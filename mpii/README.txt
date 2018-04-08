!!The code, the data and the trained models distributed here are meant solely for research purposes, and not to be used for commercial purposes. Please refer to the license (license.txt) file included with the Software.

The code demonstrates how to use the Caffe models trained for VNect [1] and also provides a basic bounding-box tracker. This code does not include the Skeleton-fitting step, and consequently there is no global root location inferred and no joint angles readily available for driving rigged characters.

If you have concerns or queries, please email Dushyant Mehta at dmehta@mpi-inf.mpg.de

Requirements:
1. Matlab
2. Caffe

Citations:
a. Models and Code: Cite [1] and [2] if using the provided models or the code.
b. Data (mpi_3dhp_ts6): The images are from Robertini et al. [3], and also a part of the MPI-INF-3DHP [2] test set.

[1] VNect: Real-time 3D Human Pose Estimation with a Single RGB Camera
	Mehta, D.; Sridhar, S.; Sotnychenko, O.; Rhodin, H.; Shafiei, M.; Seidel, H.; Xu, W.; Casas, D.; Theobalt, C. 
	ACM Transactions on Graphics, Proceedings SIGGRAPH 2017
[2] Monocular 3D Human Pose Estimation In The Wild Using Improved CNN Supervision
	Mehta, D.; Rhodin, H.; Casas, D.; Sotnychenko, O.; Xu, W.; Theobalt, C.
	arxiv:1611.09813
[3] Model-based Outdoor Performance Capture
	Robertini, N.; Casas, D.; Rhodin, H.; Seidel, H.P.S.; Theobalt, C.
	Proceedings of the Second International Conference on 3D Vision (3DV), 2016
