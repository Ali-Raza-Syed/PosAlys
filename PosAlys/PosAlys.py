import tensorflow as tf
import numpy as np

is_training = True

def create_new_conv_layer( input_data, num_input_channels, num_filters, filter_shape,
                           stride, name, activation, weight_initializer, bias_initializer ):
    conv_filter_size = [ filter_shape[ 0 ], filter_shape[ 1 ], num_input_channels, num_filters ]

    if weight_initializer == 'zeros':
        weight_init = tf.zeros_initializer()
    elif weight_initializer == 'xavier':
        weight_init = tf.contrib.layers.xavier_initializer()

    if bias_initializer == 'zeros':
        bias_init = tf.zeros_initializer()
    elif bias_initializer == 'xavier':
        bias_init = tf.contrib.layers.xavier_initializer()

    weights = tf.Variable( weight_init( conv_filter_size ), name = name + '_W' )
    bias = tf.Variable(bias_init([num_filters]), name=name + '_b')

    out_layer = tf.nn.conv2d( input_data, weights, [ 1, stride[ 0 ], stride[ 1 ], 1 ], padding = 'SAME', name = name )
    out_layer += bias
    if activation == True:
        out_layer = tf.nn.relu( out_layer )
    return out_layer

def create_new_max_pool_layer( input_data, pool_shape, stride, name ):
    #check to see whether it is [1, pool_shape[0], pool_shape[1], num_input_channels]
    ksize = [1, pool_shape[0], pool_shape[1], 1]
    strides = [1, stride[ 0 ],stride[ 1 ],  1]
    out_layer = tf.nn.max_pool( value = input_data, ksize = ksize, strides = strides,
                                padding = 'SAME', name = name )
    return out_layer

def create_block(block_name, input, filters, num_input_channels, new_stage,
                 weight_initializer, bias_initializer):
    if new_stage == True:
        first_stride = [ 2, 2 ]
    else:
        first_stride = [ 1, 1 ]
    with tf.name_scope(block_name + '2a'):
        block2a = create_new_conv_layer(input_data=input, num_input_channels=num_input_channels,
                                        num_filters=filters[ 0 ], filter_shape=[1, 1], stride=first_stride,
                                        name = block_name + '2a_conv', activation = True,
                                        weight_initializer = weight_initializer, bias_initializer = bias_initializer)
    with tf.name_scope(block_name + '2b'):
        block2b = create_new_conv_layer(input_data=block2a, num_input_channels=filters[ 0 ],
                                        num_filters=filters[ 1 ], filter_shape=[3, 3], stride=[1, 1],
                                        name= block_name + '2b_conv', activation=True,
                                        weight_initializer = weight_initializer, bias_initializer = bias_initializer )
    with tf.name_scope(block_name + '2c'):
        block2c = create_new_conv_layer(input_data=block2b, num_input_channels=filters[ 1 ],
                                        num_filters=filters[ 2 ], filter_shape=[1, 1], stride=[1, 1],
                                        name= block_name + '2c_conv', activation=False,
                                        weight_initializer = weight_initializer, bias_initializer = bias_initializer )
    with tf.name_scope('shortcut_' + block_name + '2c'):
        shortcut_block2c = create_new_conv_layer(input_data=input, num_input_channels=num_input_channels,
                                                 num_filters=filters[ 2 ], filter_shape=[1, 1], stride=first_stride,
                                                 name='shortcut_' + block_name + '2c_conv', activation=False,
                                                 weight_initializer = weight_initializer, bias_initializer = bias_initializer)
    with tf.name_scope('shortcut_add_' + block_name + '2c'):
        shortcut_add_block2c = shortcut_block2c + block2c
        shortcut_add_block2c = tf.nn.relu(shortcut_add_block2c)
    return shortcut_add_block2c

input_shape = [ 256, 256 ]
input_channels = 3
#confirm input image shape
input = tf.placeholder( tf.float32, [ None, input_shape[ 0 ], input_shape[ 1 ], input_channels ] )

with tf.name_scope( 'res1' ):
    with tf.name_scope( 'res1a_conv' ):
        res1a_conv = create_new_conv_layer( input_data = input, num_input_channels = input_channels,
                                            num_filters = 64, filter_shape = [ 7, 7 ], stride = [ 2, 2 ],
                                            name = 'res1a_conv1', activation = True,
                                            weight_initializer = 'zeros', bias_initializer = 'zeros')
    with tf.name_scope( 'res1a_max_pool' ):
        res1a_max_pool = create_new_max_pool_layer( input_data = res1a_conv, pool_shape = [ 3, 3 ],
                                                    stride = [ 2, 2 ], name = 'res1a_max_pool1' )

with tf.name_scope( 'res2' ):
    filters_res2 = [ 64, 64, 256 ]
    res2a = create_block(block_name = 'res2a', input = res1a_max_pool,
                        filters = filters_res2, num_input_channels = 64,
                        new_stage=False, weight_initializer='zeros', bias_initializer='zeros')
    res2b = create_block(block_name='res2b', input=res2a,
                        filters=filters_res2, num_input_channels=filters_res2[ 2 ],
                        new_stage=False, weight_initializer='zeros', bias_initializer='zeros')
    res2c = create_block(block_name='res2c', input=res2b,
                        filters=filters_res2, num_input_channels=filters_res2[ 2 ],
                        new_stage=False, weight_initializer='zeros', bias_initializer='zeros')

with tf.name_scope( 'res3' ):
    filters_res3 = [ 128, 128, 512 ]
    res3a = create_block(block_name = 'res3a', input = res2c,
                        filters = filters_res3, num_input_channels = filters_res2[ 2 ],
                        new_stage=True, weight_initializer='zeros', bias_initializer='zeros')
    res3b = create_block(block_name='res3b', input=res3a,
                        filters=filters_res3, num_input_channels=filters_res3[ 2 ],
                        new_stage=False, weight_initializer='zeros', bias_initializer='zeros')
    res3c = create_block(block_name='res3c', input=res3b,
                        filters=filters_res3, num_input_channels=filters_res3[ 2 ],
                        new_stage=False, weight_initializer='zeros', bias_initializer='zeros')
    res3d = create_block(block_name='res3d', input=res3c,
                        filters=filters_res3, num_input_channels=filters_res3[2],
                        new_stage=False, weight_initializer='zeros', bias_initializer='zeros')

with tf.name_scope( 'res4' ):
    filters_res4 = [ 256, 256, 1024 ]
    res4a = create_block(block_name = 'res4a', input = res3d,
                        filters = filters_res4, num_input_channels = filters_res3[ 2 ],
                        new_stage=True, weight_initializer='zeros', bias_initializer='zeros')
    res4b = create_block(block_name='res4b', input=res4a,
                        filters=filters_res4, num_input_channels=filters_res4[ 2 ],
                        new_stage=False, weight_initializer='zeros', bias_initializer='zeros')
    res4c = create_block(block_name='res4c', input=res4b,
                        filters=filters_res4, num_input_channels=filters_res4[ 2 ],
                        new_stage=False, weight_initializer='zeros', bias_initializer='zeros')
    res4d = create_block(block_name='res4d', input=res4c,
                        filters=filters_res4, num_input_channels=filters_res4[2],
                        new_stage=False, weight_initializer='zeros', bias_initializer='zeros')
    res4e = create_block(block_name='res4e', input=res4d,
                        filters=filters_res4, num_input_channels=filters_res4[ 2 ],
                        new_stage=False, weight_initializer='zeros', bias_initializer='zeros')
    res4f = create_block(block_name='res4f', input=res4e,
                        filters=filters_res4, num_input_channels=filters_res4[2],
                        new_stage=False, weight_initializer='zeros', bias_initializer='zeros')

with tf.name_scope( 'res5' ):
    #check deault bias_initializer in caffe
    filters_res5 = [ 512, 512, 1024 ]
    res5a = create_block(block_name = 'res5a', input = res4f,
                        filters = filters_res5, num_input_channels = filters_res4[ 2 ],
                        new_stage=False, weight_initializer='xavier', bias_initializer='xavier')
    with tf.name_scope('res5b2a'):
        res5b = create_new_conv_layer( input_data = res5a, num_input_channels = filters_res5[ 2 ],
                                       num_filters = 256, filter_shape = [ 1, 1 ], stride = [ 1, 1 ],
                                       name = 'res5b_conv1', activation = True,
                                       weight_initializer='xavier', bias_initializer='xavier')
    with tf.name_scope('res5b2b'):
        res5b = create_new_conv_layer(input_data=res5b, num_input_channels=256,
                                      num_filters=128, filter_shape=[3, 3], stride=[1, 1],
                                      name='res5b_conv2', activation=True,
                                      weight_initializer='xavier', bias_initializer='xavier')
    with tf.name_scope('res5b2c'):
        res5b = create_new_conv_layer(input_data=res5b, num_input_channels=128,
                                      num_filters=256, filter_shape=[1, 1], stride=[1, 1],
                                      name='res5b_conv3', activation=True,
                                      weight_initializer='xavier', bias_initializer='xavier')

with tf.name_scope('upper_layer'):
    upper_layer = tf.contrib.layers.conv2d_transpose(res5b, kernel_size=4, num_outputs=128,
                                                     activation_fn=None, stride=2,
                                                     padding='same', biases_initializer=False)

    #in code it was is_training
    upper_layer_batch_normalization = tf.layers.batch_normalization(upper_layer, scale = True, training = is_training,
                                                           name='batch_normalization')

    upper_layer_batch_normalization = tf.nn.relu(upper_layer_batch_normalization)

with tf.name_scope('order_1_joint_positions_layer'):
    #default Xavier initializtion
    #argument names different from code
    order_1_joints = tf.contrib.layers.conv2d_transpose( res5b, kernel_size = 4, num_outputs = 63,
                                                         activation_fn = None, stride = 2,
                                                         padding = 'same', biases_initializer = False)

    delta_x, delta_y, delta_z = tf.split(order_1_joints, num_or_size_splits = 3, axis = 3)

with tf.name_scope('Bone_Lengths'):
    square = tf.multiply(order_1_joints, order_1_joints, name='square')

    delta_x_square, delta_y_square, delta_z_square = tf.split(square, num_or_size_splits = 3, axis = 3)

    bone_length_square = tf.add(tf.add(delta_x_square, delta_y_square), delta_z_square)

    bone_length = tf.sqrt(bone_length_square)


concatenated = tf.concat([ upper_layer_batch_normalization, delta_x, delta_y, delta_z, bone_length],
                            axis=3, name='concatenated')

with tf.name_scope('conv_128'):
    conv_128 = create_new_conv_layer( input_data = concatenated, num_input_channels = 128 + 63 + 21,
                                      num_filters = 128, filter_shape = [ 3, 3 ], stride = [ 1, 1 ],
                                      name = 'conv_128', activation = True, weight_initializer = 'xavier',
                                      bias_initializer = 'zeros')
with tf.name_scope('conv_84'):
    conv_84 = create_new_conv_layer( input_data = conv_128, num_input_channels = 128,
                                      num_filters = 84, filter_shape = [ 1, 1 ], stride = [ 1, 1 ],
                                      name = 'conv_84', activation = False, weight_initializer = 'xavier',
                                      bias_initializer = 'zeros')

heatmap, x_location_map, y_location_map, z_location_map = tf.split(conv_84,
                                                                   num_or_size_splits = 4, axis = 3)


init_op = tf.global_variables_initializer()
with tf.Session() as sess:
    writer = tf.summary.FileWriter('logs', sess.graph)
    sess.run( init_op )
    sess.run( [heatmap, x_location_map, y_location_map, z_location_map], feed_dict = {input : np.ones( [ 10, 256, 256, 3 ] )} )
    writer.close()