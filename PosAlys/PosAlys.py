import tensorflow as tf
import numpy as np

def create_new_conv_layer( input_data, num_input_channels, num_filters, filter_shape, stride, name, activation ):
    conv_filter_size = [ filter_shape[ 0 ], filter_shape[ 1 ], num_input_channels, num_filters ]
    weights = tf.Variable( tf.truncated_normal( conv_filter_size, stddev = 0.03 ), name = name + '_W' )
    bias = tf.Variable(tf.truncated_normal([num_filters]), name=name + '_b')

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

def create_block(block_name, input, filters, num_input_channels, new_stage):
    if new_stage == True:
        first_stride = [ 2, 2 ]
    else:
        first_stride = [ 1, 1 ]
    with tf.name_scope(block_name + '2a'):
        block2a = create_new_conv_layer(input_data=input, num_input_channels=num_input_channels,
                                        num_filters=filters[ 0 ], filter_shape=[1, 1], stride=first_stride,
                                        name = block_name + '2a_conv', activation=True)
    with tf.name_scope(block_name + '2b'):
        block2b = create_new_conv_layer(input_data=block2a, num_input_channels=filters[ 0 ],
                                        num_filters=filters[ 1 ], filter_shape=[3, 3], stride=[1, 1],
                                        name= block_name + '2b_conv', activation=True)
    with tf.name_scope(block_name + '2c'):
        block2c = create_new_conv_layer(input_data=block2b, num_input_channels=filters[ 1 ],
                                        num_filters=filters[ 2 ], filter_shape=[1, 1], stride=[1, 1],
                                        name= block_name + '2c_conv', activation=False)
    with tf.name_scope('shortcut_' + block_name + '2c'):
        shortcut_block2c = create_new_conv_layer(input_data=input, num_input_channels=num_input_channels,
                                                 num_filters=filters[ 2 ], filter_shape=[1, 1], stride=first_stride,
                                                 name='shortcut_' + block_name + '2c_conv', activation=False)
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
                                            name = 'res1a_conv1', activation = True)
    with tf.name_scope( 'res1a_max_pool' ):
        res1a_max_pool = create_new_max_pool_layer( input_data = res1a_conv, pool_shape = [ 3, 3 ],
                                                    stride = [ 2, 2 ], name = 'res1a_max_pool1' )

with tf.name_scope( 'res2' ):
    filters_res2 = [ 64, 64, 256 ]
    res2a = create_block(block_name = 'res2a', input = res1a_max_pool,
                      filters = filters_res2, num_input_channels = 64, new_stage=False)
    res2b = create_block(block_name='res2b', input=res2a,
                          filters=filters_res2, num_input_channels=filters_res2[ 2 ], new_stage=False)
    res2c = create_block(block_name='res2c', input=res2b,
                          filters=filters_res2, num_input_channels=filters_res2[ 2 ], new_stage=False)

with tf.name_scope( 'res3' ):
    filters_res3 = [ 128, 128, 512 ]
    res3a = create_block(block_name = 'res3a', input = res2c,
                      filters = filters_res3, num_input_channels = filters_res2[ 2 ], new_stage=True)
    res3b = create_block(block_name='res3b', input=res3a,
                          filters=filters_res3, num_input_channels=filters_res3[ 2 ], new_stage=False)
    res3c = create_block(block_name='res3c', input=res3b,
                          filters=filters_res3, num_input_channels=filters_res3[ 2 ], new_stage=False)
    res3d = create_block(block_name='res3d', input=res3c,
                         filters=filters_res3, num_input_channels=filters_res3[2], new_stage=False)

with tf.name_scope( 'res4' ):
    filters_res4 = [ 256, 256, 1024 ]
    res4a = create_block(block_name = 'res4a', input = res3d,
                      filters = filters_res4, num_input_channels = filters_res3[ 2 ], new_stage=True)
    res4b = create_block(block_name='res4b', input=res4a,
                          filters=filters_res4, num_input_channels=filters_res4[ 2 ], new_stage=False)
    res4c = create_block(block_name='res4c', input=res4b,
                          filters=filters_res4, num_input_channels=filters_res4[ 2 ], new_stage=False)
    res4d = create_block(block_name='res4d', input=res4c,
                         filters=filters_res4, num_input_channels=filters_res4[2], new_stage=False)
    res4e = create_block(block_name='res4e', input=res4d,
                          filters=filters_res4, num_input_channels=filters_res4[ 2 ], new_stage=False)
    res4f = create_block(block_name='res4f', input=res4e,
                         filters=filters_res4, num_input_channels=filters_res4[2], new_stage=False)

with tf.name_scope( 'res5' ):
    filters_res5 = [ 512, 512, 1024 ]
    res5a = create_block(block_name = 'res5a', input = res4f,
                      filters = filters_res5, num_input_channels = filters_res4[ 2 ], new_stage=False)
    with tf.name_scope('res5b'):
        with tf.name_scope('res5b_conv1'):
            res5b = create_new_conv_layer( input_data = res5a, num_input_channels = filters_res5[ 2 ],
                                           num_filters = 256, filter_shape = [ 1, 1 ], stride = [ 1, 1 ],
                                           name = 'res5b_conv1', activation = True)
        with tf.name_scope('res5b_conv2'):
            res5b = create_new_conv_layer(input_data=res5b, num_input_channels=256,
                                          num_filters=128, ilter_shape=[3, 3], stride=[1, 1],
                                          name='res5b_conv2', activation=True)
        with tf.name_scope('res5b_conv2'):
            res5b = create_new_conv_layer(input_data=res5b, num_input_channels=128,
                                          num_filters=256, filter_shape=[1, 1], stride=[1, 1],
                                          name='res5b_conv3', activation=True)


a = res4f + 1

init_op = tf.global_variables_initializer()
with tf.Session() as sess:
    writer = tf.summary.FileWriter('logs', sess.graph)
    sess.run( init_op )
    sess.run( a, feed_dict = {input : np.ones( [ 10, 256, 256, 3 ] )} )
    writer.close()