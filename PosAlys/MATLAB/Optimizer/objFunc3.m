function y = objFunc3(pred_global_plus_d, pred_p, pred_2d, focal_length)
%only EIK implemented
    pred_global = pred_global_plus_d( :, 1 : 21 );
    d = pred_global_plus_d( :, 22 );
    
    pi_matrix = [focal_length / d(3),       0,                 0
                          0         , focal_length / d(3),     0];
    EProj_temp = ( pi_matrix * pred_global ) - pred_2d;
    
    EIK = [];
    EProj = [];
    y = [];
    for i = 1 : 21
       curr_EIK = norm( pred_global( :, i ) - d - pred_p( :, i ) );
       EIK = cat( 1, EIK, curr_EIK );
       curr_EProj = norm( EProj_temp( :, i ) );
       EProj = cat( 1, EProj, curr_EProj );
    end
    %just to make it a scalar value
    y = sum( cat( 1, EIK, EProj ));
end