function y = objFunc(pred_global_plus_d, pred_p)
%only EIK implemented
    pred_global = pred_global_plus_d( :, 1 : 21 );
    d = pred_global_plus_d( :, 22 );
    
    y = [];
    for i = 1 : 21
       y = cat( 1, y, norm( pred_global( :, i ) - d - pred_p( :, i ) ) ); 
    end
    %just to make it a scalar value
    y = sum(y);
end