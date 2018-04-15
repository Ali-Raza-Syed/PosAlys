clearvars -except imgs_info
imgs_path = 'C:\data_set\Images\mpii_human_pose_v1\images';

%%names of images in \images_paths, including name, modified date etc.
imgs_struct = dir( strcat( imgs_path, '\*.jpg' ) );

%%interested only in name field in \images_struct, so extracting it
%%and converting it into a column. Hence each name of image in separate row
imgs_names = cell2mat({imgs_struct.name}');

imgs_info_path = 'C:\data_set\Images\mpii_human_pose_v1_u12_2\mpii_human_pose_v1_u12_2';
%imgs_info = load( strcat( imgs_info_path, '\mpii_human_pose_v1_u12_1.mat' ) );

train_test_assignment = imgs_info.RELEASE.img_train;
train_annolist = imgs_info.RELEASE.annolist( logical( train_test_assignment ) );

img_name = train_annolist(1).image.name;
img = imread( strcat( imgs_path, '\', img_name ) );

%getting joints positions as cells and converting it to matrix
joint_pos_x = [ train_annolist(1).annorect(2).annopoints.point(:).x ]';
joint_pos_y = [ train_annolist(1).annorect(2).annopoints.point(:).y ]';
joint_pos = cat( 2, joint_pos_x, joint_pos_y );

center_point = [ floor( size( img, 2 ) / 2 ), floor( size( img, 1 ) / 2 ) ];
center_mat = repmat( center_point, [ size( joint_pos, 1 ), 1 ] );
joint_pos_from_center = floor( joint_pos - center_mat );

img_rows = size( img, 1 );
img_cols = size( img, 2 );

gauss_rows = -floor( img_rows / 2 ) : floor( img_rows / 2 );
gauss_cols = -floor( img_cols / 2 ) : floor( img_cols / 2 );

[ x, y ] = meshgrid( gauss_cols, gauss_rows );
%x = x';
%y = y';
sigma = 1.5;
heatmap = 0.5*pi*sigma.^2 .* exp( -( (joint_pos_from_center( 10, 1 )-x).^2 +...
                                    (joint_pos_from_center( 10, 2 ) - y).^2 ) / ( 2*sigma.^2 ) );
heatmap = heatmap * 255;

%if heatmap size inequal due to division by 2 in \gauss_rows and \gauss_cols,
%remove last row or last column
if size( heatmap, 1 ) ~= img_rows
   heatmap = heatmap( 1 : end - 1, : ); 
end
if size( heatmap, 2 ) ~= img_cols
   heatmap = heatmap( :, 1 : end - 1 ); 
end

imshow( heatmap );
figure;
markered_img = insertMarker( img, joint_pos );
imshow( markered_img );