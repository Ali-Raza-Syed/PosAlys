function [c, ceq] = constraints(pred_global_plus_d, limb_lengths)
    pred_global = pred_global_plus_d( :, 1 : 21 );
    d = pred_global_plus_d( :, 22 );
    
    c = [];
   
    ceq = get_limb_lengths( pred_global ) - limb_lengths;
end