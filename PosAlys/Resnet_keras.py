import keras
from keras.models import Sequential
from keras.layers import Conv2D, MaxPooling2D, ZeroPadding2D, Activation
from keras.models import Model
from keras.layers import Input

num_joints = 17
input_shape = ( 256, 256, 3 )
input = Input( shape = input_shape )
#conv1
res1a2a = Conv2D( filters = 64, kernel_size = 7, strides = ( 2, 2 ), padding = 'same',
                   activation = 'relu', input_shape = input_shape, name = 'res1a2a' )( input )
res1a_Max_Pool = MaxPooling2D( pool_size = ( 3, 3 ), strides = ( 2, 2 ), padding = 'same', name = 'res1a_Max_Pool' )( res1a2a )

#res2a
res2a2a = Conv2D( filters =  64, kernel_size = 1, strides = ( 1, 1 ), padding = 'same',
                   activation = 'relu', name = 'res2a2a')( res1a_Max_Pool )
res2a2b = Conv2D( filters =  64, kernel_size = 3, strides = ( 1, 1 ), padding = 'same',
                   activation = 'relu', name = 'res2a2b')( res2a2a )
res2a2c = Conv2D( filters =  256, kernel_size = 1, strides = ( 1, 1 ), padding = 'same',
                   activation = None, name = 'res2a2c')( res2a2b )
shortcut_res2a2c = Conv2D( filters = 256, kernel_size = 1, strides = ( 1, 1 ), padding = 'same',
                   activation = None, name = 'shortcut_res2a2c')( res1a_Max_Pool )
add_res2a2c = keras.layers.add( [ res2a2c, shortcut_res2a2c ], name = 'add_res2a2c' )
activation_res2a2c = Activation( 'relu' )( add_res2a2c )

#res2b
res2b2a = Conv2D( filters =  64, kernel_size = 1, strides = ( 1, 1 ), padding = 'same',
                   activation = 'relu', name = 'res2b2a')( activation_res2a2c )
res2b2b = Conv2D( filters =  64, kernel_size = 3, strides = ( 1, 1 ), padding = 'same',
                   activation = 'relu', name = 'res2b2b')( res2b2a )
res2b2c = Conv2D( filters =  256, kernel_size = 1, strides = ( 1, 1 ), padding = 'same',
                   activation = None, name = 'res2b2c')( res2b2b )
shortcut_res2b2c = Conv2D( filters = 256, kernel_size = 1, strides = ( 1, 1 ), padding = 'same',
                   activation = None, name = 'shortcut_res2b2c')( activation_res2a2c )
add_res2b2c = keras.layers.add( [ res2b2c, shortcut_res2b2c ], name = 'add_res2b2c' )
activation_res2b2c = Activation( 'relu' )( add_res2b2c )

#res2c
res2c2a = Conv2D( filters =  64, kernel_size = 1, strides = ( 1, 1 ), padding = 'same',
                   activation = 'relu', name = 'res2c2a')( activation_res2b2c )
res2c2b = Conv2D( filters =  64, kernel_size = 3, strides = ( 1, 1 ), padding = 'same',
                   activation = 'relu', name = 'res2c2b')( res2c2a )
res2c2c = Conv2D( filters =  256, kernel_size = 1, strides = ( 1, 1 ), padding = 'same',
                   activation = None, name = 'res2c2c')( res2c2b )
shortcut_res2c2c = Conv2D( filters = 256, kernel_size = 1, strides = ( 1, 1 ), padding = 'same',
                   activation = None, name = 'shortcut_res2c2c')( activation_res2b2c )
add_res2c2c = keras.layers.add( [ res2c2c, shortcut_res2c2c ], name = 'add_res2c2c' )
activation_res2c2c = Activation( 'relu' )( add_res2c2c )

#res3a
res3a2a = Conv2D( filters =  128, kernel_size = 1, strides = ( 2, 2 ), padding = 'same',
                   activation = 'relu', name = 'res3a2a')( activation_res2c2c )
res3a2b = Conv2D( filters =  128, kernel_size = 3, strides = ( 1, 1 ), padding = 'same',
                   activation = 'relu', name = 'res3a2b')( res3a2a )
res3a2c = Conv2D( filters =  512, kernel_size = 3, strides = ( 1, 1 ), padding = 'same',
                   activation = None, name = 'res3a2c')( res3a2b )
shortcut_res3a2c = Conv2D( filters = 512, kernel_size = 1, strides = ( 2, 2 ), padding = 'same',
                   activation = None, name = 'shortcut_res3a2c')( activation_res2c2c )
add_res3a2c = keras.layers.add( [ res3a2c, shortcut_res3a2c ], name = 'add_res3a2c' )
activation_res3a2c = Activation( 'relu' )( add_res3a2c )

#res3b
res3b2a = Conv2D( filters =  128, kernel_size = 1, strides = ( 1, 1 ), padding = 'same',
                   activation = 'relu', name = 'res3b2a')( activation_res3a2c )
res3b2b = Conv2D( filters =  128, kernel_size = 3, strides = ( 1, 1 ), padding = 'same',
                   activation = 'relu', name = 'res3b2b')( res3b2a )
res3b2c = Conv2D( filters =  512, kernel_size = 3, strides = ( 1, 1 ), padding = 'same',
                   activation = None, name = 'res3b2c')( res3b2b )
shortcut_res3b2c = Conv2D( filters = 512, kernel_size = 1, strides = ( 1, 1 ), padding = 'same',
                   activation = None, name = 'shortcut_res3b2c')( activation_res3a2c )
add_res3b2c = keras.layers.add( [ res3b2c, shortcut_res3b2c ], name = 'add_res3b2c' )
activation_res3b2c = Activation( 'relu' )( add_res3b2c )


#res3c
res3c2a = Conv2D( filters =  128, kernel_size = 1, strides = ( 1, 1 ), padding = 'same',
                   activation = 'relu', name = 'res3c2a')( activation_res3b2c )
res3c2b = Conv2D( filters =  128, kernel_size = 3, strides = ( 1, 1 ), padding = 'same',
                   activation = 'relu', name = 'res3c2b')( res3c2a )
res3c2c = Conv2D( filters =  512, kernel_size = 3, strides = ( 1, 1 ), padding = 'same',
                   activation = None, name = 'res3c2c')( res3c2b )
shortcut_res3c2c = Conv2D( filters = 512, kernel_size = 1, strides = ( 1, 1 ), padding = 'same',
                   activation = None, name = 'shortcut_res3c2c')( activation_res3b2c )
add_res3c2c = keras.layers.add( [ res3c2c, shortcut_res3c2c ], name = 'add_res3c2c' )
activation_res3c2c = Activation( 'relu' )( add_res3c2c )

#res3d
res3d2a = Conv2D( filters =  128, kernel_size = 1, strides = ( 1, 1 ), padding = 'same',
                   activation = 'relu', name = 'res3d2a')( activation_res3c2c )
res3d2b = Conv2D( filters =  128, kernel_size = 3, strides = ( 1, 1 ), padding = 'same',
                   activation = 'relu', name = 'res3d2b')( res3d2a )
res3d2c = Conv2D( filters =  512, kernel_size = 3, strides = ( 1, 1 ), padding = 'same',
                   activation = None, name = 'res3d2c')( res3d2b )
shortcut_res3d2c = Conv2D( filters = 512, kernel_size = 1, strides = ( 1, 1 ), padding = 'same',
                   activation = None, name = 'shortcut_res3d2c')( activation_res3c2c )
add_res3d2c = keras.layers.add( [ res3d2c, shortcut_res3d2c ], name = 'add_res3d2c' )
activation_res3d2c = Activation( 'relu' )( add_res3d2c )

#res4a
res4a2a = Conv2D( filters =  256, kernel_size = 1, strides = ( 2, 2 ), padding = 'same',
                   activation = 'relu', name = 'res4a2a')( activation_res3d2c )
res4a2b = Conv2D( filters =  256, kernel_size = 3, strides = ( 1, 1 ), padding = 'same',
                   activation = 'relu', name = 'res4a2b')( res4a2a )
res4a2c = Conv2D( filters =  1024, kernel_size = 1, strides = ( 1, 1 ), padding = 'same',
                   activation = 'relu', name = 'res4a2c')( res4a2b )
shortcut_res4a2c = Conv2D( filters = 1024, kernel_size = 1, strides = ( 2, 2 ), padding = 'same',
                   activation = None, name = 'shortcut_res4a2c')( activation_res3d2c )
add_res4a2c = keras.layers.add( [ res4a2c, shortcut_res4a2c ], name = 'add_res4a2c' )
activation_res4a2c = Activation( 'relu' )( add_res4a2c )

#res4b
res4b2a = Conv2D( filters =  256, kernel_size = 1, strides = ( 1, 1 ), padding = 'same',
                   activation = 'relu', name = 'res4b2a')( activation_res4a2c )
res4b2b = Conv2D( filters =  256, kernel_size = 3, strides = ( 1, 1 ), padding = 'same',
                   activation = 'relu', name = 'res4b2b')( res4b2a )
res4b2c = Conv2D( filters =  1024, kernel_size = 1, strides = ( 1, 1 ), padding = 'same',
                   activation = 'relu', name = 'res4b2c')( res4b2b )
shortcut_res4b2c = Conv2D( filters = 1024, kernel_size = 1, strides = ( 1, 1 ), padding = 'same',
                   activation = None, name = 'shortcut_res4b2c')( activation_res4a2c )
add_res4b2c = keras.layers.add( [ res4b2c, shortcut_res4b2c ], name = 'add_res4b2c' )
activation_res4b2c = Activation( 'relu' )( add_res4b2c )

#res4c
res4c2a = Conv2D( filters =  256, kernel_size = 1, strides = ( 1, 1 ), padding = 'same',
                   activation = 'relu', name = 'res4c2a')( activation_res4b2c )
res4c2b = Conv2D( filters =  256, kernel_size = 3, strides = ( 1, 1 ), padding = 'same',
                   activation = 'relu', name = 'res4c2b')( res4c2a )
res4c2c = Conv2D( filters =  1024, kernel_size = 1, strides = ( 1, 1 ), padding = 'same',
                   activation = 'relu', name = 'res4c2c')( res4c2b )
shortcut_res4c2c = Conv2D( filters = 1024, kernel_size = 1, strides = ( 1, 1 ), padding = 'same',
                   activation = None, name = 'shortcut_res4c2c')( activation_res4b2c )
add_res4c2c = keras.layers.add( [ res4c2c, shortcut_res4c2c ], name = 'add_res4c2c' )
activation_res4c2c = Activation( 'relu' )( add_res4c2c )

#res4d
res4d2a = Conv2D( filters =  256, kernel_size = 1, strides = ( 1, 1 ), padding = 'same',
                   activation = 'relu', name = 'res4d2a')( activation_res4c2c )
res4d2b = Conv2D( filters =  256, kernel_size = 3, strides = ( 1, 1 ), padding = 'same',
                   activation = 'relu', name = 'res4d2b')( res4d2a )
res4d2c = Conv2D( filters =  1024, kernel_size = 1, strides = ( 1, 1 ), padding = 'same',
                   activation = 'relu', name = 'res4d2c')( res4d2b )
shortcut_res4d2c = Conv2D( filters = 1024, kernel_size = 1, strides = ( 1, 1 ), padding = 'same',
                   activation = None, name = 'shortcut_res4d2c')( activation_res4c2c )
add_res4d2c = keras.layers.add( [ res4d2c, shortcut_res4d2c ], name = 'add_res4d2c' )
activation_res4d2c = Activation( 'relu' )( add_res4d2c )

#res4e
res4e2a = Conv2D( filters =  256, kernel_size = 1, strides = ( 1, 1 ), padding = 'same',
                   activation = 'relu', name = 'res4e2a')( activation_res4d2c )
res4e2b = Conv2D( filters =  256, kernel_size = 3, strides = ( 1, 1 ), padding = 'same',
                   activation = 'relu', name = 'res4e2b')( res4e2a )
res4e2c = Conv2D( filters =  1024, kernel_size = 1, strides = ( 1, 1 ), padding = 'same',
                   activation = 'relu', name = 'res4e2c')( res4e2b )
shortcut_res4e2c = Conv2D( filters = 1024, kernel_size = 1, strides = ( 1, 1 ), padding = 'same',
                   activation = None, name = 'shortcut_res4e2c')( activation_res4d2c )
add_res4e2c = keras.layers.add( [ res4e2c, shortcut_res4e2c ], name = 'add_res4e2c' )
activation_res4e2c = Activation( 'relu' )( add_res4e2c )


#res4f
res4f2a = Conv2D( filters =  256, kernel_size = 1, strides = ( 1, 1 ), padding = 'same',
                   activation = 'relu', name = 'res4f2a')( activation_res4e2c )
res4f2b = Conv2D( filters =  256, kernel_size = 3, strides = ( 1, 1 ), padding = 'same',
                   activation = 'relu', name = 'res4f2b')( res4f2a )
res4f2c = Conv2D( filters =  1024, kernel_size = 1, strides = ( 1, 1 ), padding = 'same',
                   activation = 'relu', name = 'res4f2c')( res4f2b )
shortcut_res4f2c = Conv2D( filters = 1024, kernel_size = 1, strides = ( 1, 1 ), padding = 'same',
                   activation = None, name = 'shortcut_res4f2c')( activation_res4e2c )
add_res4f2c = keras.layers.add( [ res4f2c, shortcut_res4f2c ], name = 'add_res4f2c' )
activation_res4f2c = Activation( 'relu' )( add_res4f2c )


#Vnect model start
#res5a
res5a2a = Conv2D( filters =  512, kernel_size = 1, strides = ( 1, 1 ), padding = 'same',
                   activation = 'relu', name = 'res5a2a')( activation_res4f2c )
res5a2b = Conv2D( filters =  512, kernel_size = 3, strides = ( 1, 1 ), padding = 'same',
                   activation = 'relu', name = 'res5a2b')( res5a2a )
res5a2c = Conv2D( filters =  1024, kernel_size = 1, strides = ( 1, 1 ), padding = 'same',
                   activation = None, name = 'res5a2c')( res5a2b )
shortcut_res5a2c = Conv2D( filters = 1024, kernel_size = 1, strides = ( 1, 1 ), padding = 'same',
                   activation = None, name = 'shortcut_res5a2c')( activation_res4f2c )
add_res5a2c = keras.layers.add( [ res5a2c, shortcut_res5a2c ], name = 'add_res5a2c' )
activation_res5a2c = Activation( 'relu' )( add_res5a2c )

#res5b
res5b2a = Conv2D( filters =  256, kernel_size = 1, strides = ( 1, 1 ), padding = 'same',
                   activation = 'relu', name = 'res5b2a')( activation_res5a2c )
res5b2b = Conv2D( filters =  128, kernel_size = 3, strides = ( 1, 1 ), padding = 'same',
                   activation = 'relu', name = 'res5b2b')( res5b2a )
res5b2c = Conv2D( filters =  256, kernel_size = 1, strides = ( 1, 1 ), padding = 'same',
                   activation = 'relu', name = 'res5b2c')( res5b2b )
deconvolution_up = keras.layers.Conv2DTranspose( filters = 128, kernel_size = 4, strides = ( 2, 2 ), activation = 'relu',
                                         padding = 'same')( res5b2c )

deconvolution_down = keras.layers.Conv2DTranspose( filters = 3 * num_joints, kernel_size = 4, strides = ( 2, 2 )
                                                   , activation = None, padding = 'same')( res5b2c )

sqr = keras.backend.square( deconvolution_down )
square_x = keras.layers.Lambda( lambda sqr : sqr[ :, :, :, 0 : num_joints ] )( sqr )
square_y = keras.layers.Lambda( lambda sqr : sqr[ :, :, :, num_joints : 2 * num_joints ] )( sqr )
square_z = keras.layers.Lambda( lambda sqr : sqr[ :, :, :, 2 * num_joints : 3 * num_joints ] )( sqr )

add_sqr = keras.layers.Add( )( [ square_x, square_y ] )
sqr_root = keras.backend.sqrt( add_sqr )


model = Model( inputs = input, outputs = sqr_root )




model.compile(loss=keras.losses.categorical_crossentropy,
              optimizer=keras.optimizers.SGD(lr=0.01),
              metrics=['accuracy'])


print( model.summary() )