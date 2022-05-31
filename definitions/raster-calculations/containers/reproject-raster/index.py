import numpy as np
import rasterio
from rasterio.warp import calculate_default_transform, reproject, Resampling
from decouple import config
import os

input_artifacts_folder = config('INPUT_ARTIFACTS_PATH')
input_parameters_folder = config('INPUT_PARAMETERS_PATH')
output_artifacts_folder = config('OUTPUT_ARTIFACTS_PATH')

source = os.path.join(input_artifacts_folder, "source_raster")
output = os.path.join(output_artifacts_folder, "output_raster")

# Target Projection
f = open(os.path.join(input_parameters_folder, "target_projection"), "r")
dst_crs = {'init': f.read()}
f.close()

# Source Projection
f = open(os.path.join(input_parameters_folder, "source_projection"), "r")
src_crs = {'init': f.read()}
f.close()

"""
This code obtained from
https://github.com/rasterio/rasterio/blob/master/examples/reproject.py
"""

with rasterio.Env(CHECK_WITH_INVERT_PROJ=True):
    with rasterio.open(source) as src:
        profile = src.profile

        # Calculate the ideal dimensions and transformation in the new crs
        dst_affine, dst_width, dst_height = calculate_default_transform(
            src.crs, dst_crs, src.width, src.height, *src.bounds)

        # update the relevant parts of the profile
        profile.update({
            'crs': dst_crs,
            'transform': dst_affine,
            'width': dst_width,
            'height': dst_height
        })

        # Reproject and write each band
        with rasterio.open(output, 'w', **profile) as dst:
            for i in range(1, src.count + 1):
                src_array = src.read(i)
                dst_array = np.empty((dst_height, dst_width), dtype='int32')

                reproject(
                    # Source parameters
                    source=src_array,
                    src_crs=src_crs,
                    src_transform=src.transform,
                    # Destination paramaters
                    destination=dst_array,
                    dst_transform=dst_affine,
                    dst_crs=dst_crs,
                    # Configuration
                    resampling=Resampling.nearest,
                    num_threads=2)

                dst.write(dst_array, i)