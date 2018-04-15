clearvars -except imgs_info
imgs_path = 'C:\data_set\Images\mpii_human_pose_v1\images';

%%names of images in \images_paths, including name, modified date etc.
imgs_struct = dir( strcat( imgs_path, '\*.jpg' ) );

%%interested only in name field in \images_struct, so extracting it
%%and converting it into a column. Hence each name of image in separate row
imgs_names = cell2mat({imgs_struct.name}');

imgs_info_path = 'C:\data_set\Images\mpii_human_pose_v1_u12_2\mpii_human_pose_v1_u12_2';
%imgs_info = load( strcat( imgs_info_path, '\mpii_human_pose_v1_u12_1.mat' ) );

%getting training data info
train_test_assignment = imgs_info.RELEASE.img_train;
train_annolist = imgs_info.RELEASE.annolist( logical( train_test_assignment ) );

num_imgs = size( train_annolist, 2 );
%curr_img_heatmaps (hm_rows, hm_cols, joint_index, person_in_image_index, img_index)
curr_img_heatmaps = [];
for img_index = 1 : num_imgs
    img_index
    img_name = train_annolist(img_index).image.name;
    img = imread( strcat( imgs_path, '\', img_name ) );
    num_persons_img = size( train_annolist( img_index ).annorect, 2 );
    person_heatmaps = [];
    for person_index = 1 : num_persons_img
        %getting joints positions as cells and converting it to matrix
        %x is column, y is rows
        joint_pos_x = [ train_annolist(img_index).annorect(person_index).annopoints.point(:).x ]';
        joint_pos_y = [ train_annolist(img_index).annorect(person_index).annopoints.point(:).y ]';
        joint_pos = cat( 2, joint_pos_x, joint_pos_y );

        center_point = [ floor( size( img, 2 ) / 2 ), floor( size( img, 1 ) / 2 ) ];
        center_mat = repmat( center_point, [ size( joint_pos, 1 ), 1 ] );
        joint_pos_from_center = floor( joint_pos - center_mat );

        img_rows = size( img, 1 );
        img_cols = size( img, 2 );

        gauss_rows = -floor( img_rows / 2 ) : floor( img_rows / 2 );
        gauss_cols = -floor( img_cols / 2 ) : floor( img_cols / 2 );

        %x is column and y is rows
        [ x, y ] = meshgrid( gauss_cols, gauss_rows );

        sigma = 1.5;
        num_joints = size( joint_pos, 1 );
        curr_person_heatmaps = [];
        for joint_index = 1 : num_joints
            curr_heatmap = 0.5*pi*sigma.^2 .* exp( -( (joint_pos_from_center( joint_index, 1 )-x).^2 +...
                                                (joint_pos_from_center( joint_index, 2 ) - y).^2 ) /...
                                                ( 2*sigma.^2 ) );
            curr_person_heatmaps = cat( 3, curr_person_heatmaps, curr_heatmap );
        end
        %curr_person_heatmaps = curr_person_heatmaps * 255;

        %if heatmap size inequal due to division by 2 in \gauss_rows and \gauss_cols,
        %remove last row or last column
        if size( curr_person_heatmaps, 1 ) ~= img_rows
           curr_person_heatmaps = curr_person_heatmaps( 1 : end - 1, :, : ); 
        end
        if size( curr_person_heatmaps, 2 ) ~= img_cols
           curr_person_heatmaps = curr_person_heatmaps( :, 1 : end - 1, : ); 
        end
        person_heatmaps = cat( 4, person_heatmaps, curr_person_heatmaps );
    end
    curr_img_heatmaps = cat( 5, curr_img_heatmaps, person_heatmaps );
end

imshow( person_heatmaps( :, :, 10, 2, 1) );