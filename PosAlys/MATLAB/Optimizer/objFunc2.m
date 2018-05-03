function y = objFunc2(pred_global_plus_d, pred_p, prev_joints, prev_prev_joints, rate)
%Objective with EIK and ESmooth (not working properly, infinite drift)
    pred_global = pred_global_plus_d( :, 1 : 21 );
    d = pred_global_plus_d( :, 22 );
    
    prev_velocity = ( prev_joints - prev_prev_joints ) * rate;
    curr_velocity = ( pred_global - prev_joints ) * rate;
    acceleration = ( curr_velocity - prev_velocity ) * rate;
    
    EIK = [];
    ESmooth = [];
    for i = 1 : 21
       curr_EIK = norm( pred_global( :, i ) - d - pred_p( :, i ) );
       curr_ESmooth = norm( acceleration( :, i ) );
       
       EIK = cat( 1, EIK, curr_EIK );
       ESmooth = cat( 1, ESmooth, curr_ESmooth );
    end
    %just to make it a scalar value
    y = sum( cat(1, 1 .* EIK, 0.07 .* ESmooth ) );
end