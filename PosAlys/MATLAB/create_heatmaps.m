clear;
imgs_path = 'C:\data_set\Images\mpii_human_pose_v1\images';

%%names of images in \images_paths, including name, modified date etc.
%imgs_struct = dir( strcat( imgs_path, '\*.jpg' ) );

%%interested only in name field in \images_struct, so extracting it
%%and converting it into a column. Hence each name of image in separate row
%imgs_names = cell2mat({imgs_struct.name}');

imgs_info_path = 'C:\data_set\Images\mpii_human_pose_v1_u12_2\mpii_human_pose_v1_u12_2';
imgs_info = load( strcat( imgs_info_path, '\mpii_human_pose_v1_u12_1.mat' ) );

%getting training data info
train_test_assignment = imgs_info.RELEASE.img_train;
train_annolist = imgs_info.RELEASE.annolist( logical( train_test_assignment ) );

heatmaps_dir = 'C:\data_set\Images\mpii';

num_imgs_one_go = 2;
num_batches = floor( size( train_annolist, 2 ) / num_imgs_one_go ) + 1;

if exist('last_successful_batch_num.mat', 'file') == 2
    starting_batch_idx = load('last_successful_batch_num.mat');
    starting_batch_idx = starting_batch_idx.batch_idx;
else
    starting_batch_idx = 1;
end

for batch_idx = starting_batch_idx : num_batches
    
    next_starting_img_idx = batch_idx * num_imgs_one_go + 1
    if batch_idx ~= num_batches
        curr_train_annolist = train_annolist( next_starting_img_idx : next_starting_img_idx + ...
                                                                       num_imgs_one_go - 1 );
    else
        curr_train_annolist = train_annolist( next_starting_img_idx : next_starting_img_idx + ...
                                              mod( size( train_annolist, 2 ) / num_imgs_one_go ) );
    end
    num_imgs = size( curr_train_annolist, 2 );
    heatmaps = cell( 1, num_imgs );
    for img_idx = 1 : num_imgs
        
        img_name = curr_train_annolist(img_idx).image.name;
        img = imread( strcat( imgs_path, '\', img_name ) );

        img_rows = size( img, 1 );
        img_cols = size( img, 2 );

        gauss_rows = -floor( img_rows / 2 ) : floor( img_rows / 2 );
        gauss_cols = -floor( img_cols / 2 ) : floor( img_cols / 2 );

        %x is column and y is rows
        [ x, y ] = meshgrid( gauss_cols, gauss_rows );

        sigma = 1.5;

        num_persons_img = size( curr_train_annolist( img_idx ).annorect, 2 );
        img_heatmaps = cell( 1,  num_persons_img);
        for person_idx = 1 : num_persons_img
            joint_pos_x = [ curr_train_annolist(img_idx).annorect(person_idx).annopoints.point(:).x ]';
            joint_pos_y = [ curr_train_annolist(img_idx).annorect(person_idx).annopoints.point(:).y ]';
            joint_pos = cat( 2, joint_pos_x, joint_pos_y );

            center_point = [ floor( size( img, 2 ) / 2 ), floor( size( img, 1 ) / 2 ) ];
            center_mat = repmat( center_point, [ size( joint_pos, 1 ), 1 ] );
            joint_pos_from_center = floor( joint_pos - center_mat );

            num_joints = size( joint_pos, 1 );

            curr_person_heatmaps = cell( size( gauss_rows, 2 ), size( gauss_cols, 2 ), num_joints );
            for joint_index = 1 : num_joints
                curr_heatmap = num2cell( 0.5*pi*sigma.^2 .* exp( -( (joint_pos_from_center( joint_index, 1 )-x).^2 +...
                                                    (joint_pos_from_center( joint_index, 2 ) - y).^2 ) /...
                                                    ( 2*sigma.^2 ) ) );
                curr_person_heatmaps(:, :, joint_index) = curr_heatmap;
            end

            if size( curr_person_heatmaps, 1 ) ~= img_rows
               curr_person_heatmaps = curr_person_heatmaps(1 : end - 1, :, : ); 
            end
            if size( curr_person_heatmaps, 2 ) ~= img_cols
               curr_person_heatmaps = curr_person_heatmaps(:, 1 : end - 1, : ); 
            end

            img_heatmaps(person_idx) = {curr_person_heatmaps};
        end
        heatmaps(img_idx) = {img_heatmaps};
    end

    for img_idx = 1 : num_imgs
        display('creating dir')
        curr_img_heatmaps = heatmaps{1, img_idx};
        img_name = curr_train_annolist( img_idx ).image.name;
        img_name = img_name(1 : end - 4);
        %mkdir heatmaps_dir img_name;
        num_persons_img = size( curr_train_annolist( img_idx ).annorect, 2 );
        for person_idx = 1 : num_persons_img
            person_path = strcat( heatmaps_dir, '\', img_name );
            person_name = strcat( 'person_', num2str( person_idx ) );
            mkdir( strcat( person_path, '/', person_name ) );
            curr_person_heatmaps = curr_img_heatmaps{ 1, person_idx };
            num_joints = size( curr_train_annolist( img_idx ).annorect( person_idx ).annopoints.point, 2 );
            for joint_idx = 1 : num_joints
                %cell2mat( curr_person_heatmaps( :, :, joint_idx ) );
                path = strcat(heatmaps_dir, '\', img_name, '\', 'person_', num2str(person_idx),'\');
                name = strcat('joint_', num2str( joint_idx ), '.jpg');
                imwrite( cell2mat( curr_person_heatmaps( :, :, joint_idx ) ), strcat( path, name ) );
            end
        end
    end
    next_starting_img_idx = next_starting_img_idx + num_imgs_one_go;
    save('last_successful_batch_num.mat', 'batch_idx');
    clearvars train_test_assignment curr_person_heatmaps curr_img_heatmaps curr_heatmap
end