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
addpath('./matlab')
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

vid_obj = VideoReader('test.mp4');

for i = 1 : 60
   readFrame(vid_obj); 
end

while hasFrame(vid_obj)

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
        pred_p(1,k) = 100* xmap(max(floor(x/hm_factor),1), max(floor(y/hm_factor),1) ,k);
        pred_p(2,k) = 100* ymap(max(floor(x/hm_factor),1), max(floor(y/hm_factor),1) ,k);
        pred_p(3,k) = 100* zmap(max(floor(x/hm_factor),1), max(floor(y/hm_factor),1) ,k);
    end
    %Subtract the root location just to be safe.
    pred_p = bsxfun(@minus, pred_p, pred_p(:,15));

    %Plot the predicted Pose       
    figure(1);
    hp = subplot(1, 2, 1);
    cla(hp);
    hold on;
    mpii_vnect_plot_skeleton(pred_p', o1, 20)
    plot3( pred_p(1,[3:5,9:11]), pred_p(2,[3:5,9:11]), pred_p(3,[3:5,9:11]) ,'ro');
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
%     hp = subplot(1, 3, 3);
%     cla(hp);
%     hold on;
%     mpii_vnect_plot_skeleton(pred_p', o1, 20)
%     plot3( pred_p(1,[3:5,9:11]), pred_p(2,[3:5,9:11]), pred_p(3,[3:5,9:11]) ,'ro');
%     hold off;
%     view([0,-45]);
%     axis([-700, 700, -800, 800, -700, 700])   
%     hold off;

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
  %w = waitforbuttonpress;
    for i = 1 : 5
        readFrame(vid_obj); 
    end 
end

