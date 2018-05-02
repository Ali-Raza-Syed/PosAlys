clear;
imgs_path = 'D:\PosAlys\data_set\MPII_2D\Images\mpii_human_pose_v1\images';
imgs_info_path = 'D:\PosAlys\data_set\MPII_2D\Images\mpii_human_pose_v1_u12_2\mpii_human_pose_v1_u12_2';
imgs_info = load( strcat( imgs_info_path, '\mpii_human_pose_v1_u12_1.mat' ) );

%%names of images in \images_paths, including name, modified date etc.
imgs_struct = dir( strcat( imgs_path, '\*.jpg' ) );

%%interested only in name field in \images_struct, so extracting it
%%and converting it into a column. Hence each name of image in separate row
imgs_names_folder = cell2mat({imgs_struct.name}');

imgs_names = [];
for i = 1 :  size(imgs_info.RELEASE.annolist, 2)
    imgs_names = cat(1, imgs_names, imgs_info.RELEASE.annolist(i).image.name );
end

not_present_idx = [];
for i = 1 :  size(imgs_names, 1)
    if mod(i, 100) == 0
        i
    end
    bara_name = imgs_names( i, 1 : 13 );
    found = 0;
    for j = 1 :  size(imgs_names_folder, 1 )
        chotta_name = imgs_names_folder( j, 1 : 13 );
        if chotta_name == bara_name
            found = 1;
            break;
        end
    end
    if found == 0
        not_present_idx = cat( 1, not_present_idx, i );
    end
end

% 
% for i = 1 : size (imgs_names_folder, 1)
%     if mod( i, 1000 ) == 0
%        i 
%     end
%     name = imgs_names_folder( i,  1 : 13 );
%     img = imread(strcat( 'D:\PosAlys\data_set\MPII_2D\Images\mpii_human_pose_v1\images\', ...
%                             name ) );
%     imwrite ( img, strcat( 'D:\PosAlys\data_set\MPII_2D\Images\images_updated\', name, '.jpg' ) );
% end