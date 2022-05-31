import requests
from decouple import config
import os

input_parameters_folder = config('INPUT_PARAMETERS_PATH')
output_artifacts_folder = config('OUTPUT_ARTIFACTS_PATH')

#INPUTS
f = open(os.path.join(input_parameters_folder, "dem_url"), "r")
dem_url=f.read()
f.close()

r = requests.get(dem_url)
open(os.path.join(output_artifacts_folder, "dem"), 'wb').write(r.content)