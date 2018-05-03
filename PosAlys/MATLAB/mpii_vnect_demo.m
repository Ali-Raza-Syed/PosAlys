% Example code for VNect models. This is not the complete VNect system,
% and only meant to show how to use the trained Caffe models. The code,
% models and data supplied as a part of the package are not for commercial use.
% Please refer to the license (license.txt) file distributed with the Software.

%% Flags
% Use multiple scales in the first frame to ensure proper detection and
% initialization of the BB tracker, then set the flag to true to start
% tracking
CROP_TRACKING_MODE = false; 
IS_FIRST_FRAME = true;
%% Global Params
CROP_SIZE = 368; %384;
CROP_RECT = [0, 0, CROP_SIZE, CROP_SIZE];
hm_factor = 8;
VISUALIZE_HEATMAPS = false;
%Point to your matcaffe
addpath('./matlab');
addpath('./Optimizer');
addpath('./One_Euro_Filter');
addpath('./util');
addpath('./external');
img_base_path = './data';
img_set = 'mpii_3dhp_ts6';
%Get the joint parents and the labels. The extended set also contains hand
%and feet tips which may not be as stable. Use the first 17 in that case.
[~,o1,~,relevant_labels] = mpii_vnect_get_joints('extended');  

%% Variables
CROP_SCALE = 1.0;
pad_offset(1:2) = 0;

%%
caffe.set_mode_cpu();
% caffe.set_mode_gpu()
% caffe.set_device(0)
caffe.reset_all();
net = caffe.Net('./models/vnect_net.prototxt', './models/vnect_model.caffemodel', 'test');

vid_obj = VideoReader('test4.mp4');

 for i = 1 : 2
    readFrame(vid_obj); 
 end
 
one_euro_pred_2d = oneEuro;
one_euro_pred_2d.mincutoff = 1.7;
one_euro_pred_2d.beta = 0.3;

one_euro_pred_p = oneEuro;
one_euro_pred_p.mincutoff = 0.8;
one_euro_pred_p.beta = 0.4;

one_euro_global_joints = oneEuro;
one_euro_global_joints.mincutoff = 20;
one_euro_global_joints.beta = 0.4;

rate = 30;

csv_matrix = [];

no_of_frames_to_determine_limb_lengths = 5;
frame_no = 0;

%limb_lengths = [dist(J1_at_frame=1, J2_at_frame=1), dist(J2_at_frame=1, J3_at_frame=1), ...
%                dist(J1_at_frame=2, J2_at_frame=2), dist(J2_at_frame=2, J3_at_frame=2), ...
%                                              .
%                                              .
%                                              .                                            ]
limb_lengths = [];

while hasFrame(vid_obj)
    frame_no = frame_no + 1
    %img_path = fullfile(img_base_path, img_set, sprintf('cam5_frame%06d.jpg', i));
    img = readFrame(vid_obj);
    img = permute(double(img)/255.0 - 0.4, [2,1,3]);
    img = img(:,:,[3 2 1]);
    img = imresize(img, [848, 448]);

    if(~CROP_TRACKING_MODE)
      scales = 1:-0.2:0.6;
      box_size = [size(img,1),size(img,2)];
      CROP_TRACKING_MODE = true;
    else
        scales = [1.0,0.7];
        box_size = [CROP_SIZE, CROP_SIZE];
        crop_offset = int32(floor(40.0 / CROP_SCALE)); 
        %Get 2D locations in the previous frame
        min_crop = (int32(CROP_RECT(1:2) + min(pred_2d, [], 2)'/CROP_SCALE) - int32(pad_offset/CROP_SCALE)) - crop_offset;
        max_crop = (int32(CROP_RECT(1:2) + max(pred_2d, [], 2)'/CROP_SCALE) - int32(pad_offset/CROP_SCALE)) + crop_offset;
        old_crop = CROP_RECT;

        CROP_RECT(1:2) = max(min_crop, int32([1, 1]));
        CROP_RECT(3:4) = min(max_crop, int32([size(img, 1), size(img,2)]));
        CROP_RECT(3:4) = CROP_RECT(3:4) - CROP_RECT(1:2);

        %Temporal smoothing of the crops
        if(~IS_FIRST_FRAME)
            mu = 0.8;
            CROP_RECT = (1-mu)* CROP_RECT + mu * old_crop;
            IS_FIRST_FRAME = false;
        end
        
        img = imcrop(img, CROP_RECT([2,1,4,3]));
        CROP_SCALE = (CROP_SIZE-2) / max(size(img, 1), size(img, 2));
        img = imresize(img, CROP_SCALE);

        if(size(img,1) > size(img,2))
            pad_offset(1) = 0;
            pad_offset(2) = (CROP_SIZE-size(img,2)) / 2;
        else
            pad_offset(2) = 0;
            pad_offset(1) = (CROP_SIZE-size(img,1)) / 2;
        end
        img = mpii_vnect_pad_image(img, box_size);

    end

    %Once we know the crop, get the imaage ready to be fed into the network
    data = zeros(box_size(1), box_size(2), 3, length(scales));
    for si = 1:length(scales)
      data(:,:,:,si) = mpii_vnect_pad_image(imresize(img, scales(si)), box_size);

    end

    net.blobs('data').reshape([size(data,1) size(data,2) size(data,3) size(data,4)]);
    net.forward({data(:,:,:,:)});

    %Get the heatmaps and the location maps from the network
    output_heatmap = net.blobs('heatmap').get_data();
    output_xmap = net.blobs('x_heatmap').get_data();
    output_ymap = net.blobs('y_heatmap').get_data();
    output_zmap = net.blobs('z_heatmap').get_data();

    %Housekeeping for the next step
    hm_size = box_size/hm_factor; %or size(output_heatmap); 1 and 2
    heatmap = zeros(hm_size(1), hm_size(2), size(output_heatmap,3));
    xmap = zeros(hm_size(1), hm_size(2), size(output_zmap,3));
    ymap = zeros(hm_size(1), hm_size(2), size(output_zmap,3));
    zmap = zeros(hm_size(1), hm_size(2), size(output_zmap,3));

    %Since the predicted heatmaps and location maps are at different
    %scales,they need to be rescaled and averaged
    for si = 1:length(scales)
      s_hm = imresize(output_heatmap(:,:,:,si), 1.0/scales(si), 'bilinear');
      s_xhm = imresize(output_xmap(:,:,:,si), 1.0/scales(si), 'bilinear');
      s_yhm = imresize(output_ymap(:,:,:,si), 1.0/scales(si), 'bilinear');
      s_zhm = imresize(output_zmap(:,:,:,si), 1.0/scales(si), 'bilinear');
      mid_pt = size(s_hm)/2;
      heatmap = heatmap + s_hm( (mid_pt(1) -floor(hm_size(1)/2)+1): (mid_pt(1) +ceil(hm_size(1)/2)), (mid_pt(2) -floor(hm_size(2)/2)+1): (mid_pt(2) +ceil(hm_size(2)/2)),:);
      xmap = xmap + s_xhm( (mid_pt(1) -floor(hm_size(1)/2)+1): (mid_pt(1) +ceil(hm_size(1)/2)), (mid_pt(2) -floor(hm_size(2)/2)+1): (mid_pt(2) +ceil(hm_size(2)/2)),:);
      ymap = ymap + s_yhm( (mid_pt(1) -floor(hm_size(1)/2)+1): (mid_pt(1) +ceil(hm_size(1)/2)), (mid_pt(2) -floor(hm_size(2)/2)+1): (mid_pt(2) +ceil(hm_size(2)/2)),:);
      zmap = zmap + s_zhm( (mid_pt(1) -floor(hm_size(1)/2)+1): (mid_pt(1) +ceil(hm_size(1)/2)), (mid_pt(2) -floor(hm_size(2)/2)+1): (mid_pt(2) +ceil(hm_size(2)/2)),:);
    end
    
    %Final heatmaps and location maps, from which we can infer the 2D and
    %3D pose
    heatmap = heatmap/length(scales);
    xmap = xmap/length(scales);
    ymap = ymap/length(scales);
    zmap = zmap/length(scales);

% %       Visualize the x/y/z maps
%         figure(3); 
%         for i = 1:17  
%             subplot(2,2,1); imagesc(permute(output_xmap(:,:,i,2),[2,1,3])); colorbar; caxis([-8 8]); 
%             subplot(2,2,2); imagesc(permute(output_ymap(:,:,i,2),[2,1,3])); colorbar; caxis([-8 8]); 
%             subplot(2,2,3); imagesc(permute(output_zmap(:,:,i,2),[2,1,3])); colorbar;caxis([-8 8]); 
%             subplot(2,2,4); imshow(permute(img(:,:,[3,2,1]), [2,1,3])+0.4);
%             waitforbuttonpress;
%         end

    pred_p = zeros(3,size(heatmap,3));
    pred_2d = zeros(2, size(pred_p,2));

    hm = zeros(box_size(2), box_size(1), size(heatmap,3));
    % Take the maximas in the heatmaps as the 2D predictions. You can
    % substitue this with a function guided by the distance from the
    % root joint. Use the maxima locations to get the 3D joint locations
    for k = 1:size(heatmap,3)
        hm(:,:,k) = imresize(permute(heatmap(:,:,k),[2,1,3]), hm_factor);
        [~,max_idx] = max(reshape(hm(:,:,k),1,[]));
        [y,x] = ind2sub(size(hm(:,:,k)), max_idx(1));
        pred_2d(1:2,k) = [x,y];
    end
    
    %Applying one euro filter on pred_2d
    pred_2d = one_euro_pred_2d.filter(pred_2d, rate);
    
    for k = 1:size(heatmap,3)
        temp = pred_2d(1:2,k);
        x = temp(1);
        y = temp(2);
        pred_p(1,k) = 100* xmap(max(floor(x/hm_factor),1), max(floor(y/hm_factor),1) ,k);
        pred_p(2,k) = 100* ymap(max(floor(x/hm_factor),1), max(floor(y/hm_factor),1) ,k);
        pred_p(3,k) = 100* zmap(max(floor(x/hm_factor),1), max(floor(y/hm_factor),1) ,k);
    end
    
    %Applying one euro filter on pred_p
    pred_p = one_euro_pred_p.filter(pred_p, rate);
    
    %Subtract the root location just to be safe.
    pred_p = bsxfun(@minus, pred_p, pred_p(:,15));
   
    
%     %Plot the predicted Pose       
%     figure(1);
%     hp = subplot(1, 2, 1);
%     cla(hp);
%     hold on;
%     mpii_vnect_plot_skeleton(pred_p', o1, 20)
%     plot3( pred_p(1,[3:5,9:11]), pred_p(2,[3:5,9:11]), pred_p(3,[3:5,9:11]) ,'ro');
%     hold off;
%     view([0,-90]);
%     axis([-700, 700, -800, 800, -700, 700])
% 
%     hp = subplot(1, 2, 2);
%     cla(hp);
%     imshow(permute(img(:,:,[3,2,1]), [2,1,3])+0.4);
%     hold on;
%     c = colormap('hsv');
%     for k = 1:16
%         plot(pred_2d(1,k),pred_2d(2,k),'.','Markersize', 15, 'Color', c(k*4, :));
%     end

    %Display Heatmaps 
    if(VISUALIZE_HEATMAPS)
     figure(2);
     for k = 1:21
        subplot(4,6, k);
        alpha(0);
        imshow(permute(img(:,:,[3,2,1]), [2,1,3])+0.4);
        hold on;
        colormap('jet');
        imagesc(hm(:,:,k));
        plot(pred_2d(1,k),pred_2d(2,k),'.g','Markersize', 10);
        hold off;
        title(relevant_labels{k},'Interpreter', 'none');
        alpha(0.3);
     end
    end
  drawnow
  
  
  if frame_no <= no_of_frames_to_determine_limb_lengths
      limb_lengths = cat(1, get_limb_lengths( pred_p ) );
      if frame_no == 1
        prev_prev_joints = pred_p; 
      end
  
      if frame_no == 2
        prev_joints = pred_p; 
      end
  else
      limb_lengths = mean( limb_lengths, 1 );
   
      x0 = cat( 2, pred_p, [ 1; 1; 1 ] );
      options = optimset('Largescale','off','Display','iter');
      [x, values] = fmincon( @(x)objFunc(x, pred_p), x0, [], [], [], [], [], [], ...
                            @(x)constraints(x, limb_lengths), [] );
                        
%       [x, values] = fmincon( @(x)objFunc2(x, pred_p, prev_joints, prev_prev_joints, rate), ...
%                             x0, [], [], [], [], [], [], ...
%                             @(x)constraints(x, limb_lengths), [] );

%       focal_length = 28;
%       [x, values] = fmincon( @(x)objFunc3(x, pred_p, pred_2d, focal_length), x0, [], [], [], [], [], [], ...
%                             @(x)constraints(x, limb_lengths), [] );
      
      global_joints = pred_p;
      %global_joints_plus_d = x;
      %global_joints = global_joints_plus_d( :, 1 : 21 );
      %d = global_joints_plus_d( :, 22 );
      
      global_joints = one_euro_global_joints.filter(global_joints, rate);
      
      csv_matrix = cat(1, csv_matrix, reshape( global_joints, 1, [] ) );
      
      prev_prev_joints = prev_joints;
      prev_joints = global_joints;
      
      %plot
      %Plot the predicted Pose       
        figure(1);
        hp = subplot(1, 2, 1);
        cla(hp);
        hold on;
        mpii_vnect_plot_skeleton(global_joints', o1, 20)
        plot3( global_joints(1,[3:5,9:11]), global_joints(2,[3:5,9:11]), global_joints(3,[3:5,9:11]) ,'ro');
        hold off;
        view([0,-90]);
        axis([-700, 700, -800, 800, -700, 700])

        hp = subplot(1, 2, 2);
        cla(hp);
        imshow(permute(img(:,:,[3,2,1]), [2,1,3])+0.4);
        hold on;
        c = colormap('hsv');
        for k = 1:16
            plot(pred_2d(1,k),pred_2d(2,k),'.','Markersize', 15, 'Color', c(k*4, :));
        end
      
  end
  
  
  
  %w = waitforbuttonpress;
end

