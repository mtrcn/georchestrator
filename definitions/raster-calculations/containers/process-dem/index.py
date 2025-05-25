#!/usr/bin/env python3
"""
DEM Terrain Analysis Script (No External Dependencies)
Generates slope, aspect, and shaded relief from Digital Elevation Model (DEM) data.
Uses only numpy and standard library
"""

import numpy as np
import os
import sys
import logging
from typing import Tuple, Dict, Any

# Setup logging
# Create handlers for different log levels
stdout_handler = logging.StreamHandler(sys.stdout)
stdout_handler.setLevel(logging.INFO)
stdout_handler.addFilter(lambda record: record.levelno <= logging.WARNING)

stderr_handler = logging.StreamHandler(sys.stderr)
stderr_handler.setLevel(logging.ERROR)

# Configure root logger
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(levelname)s - %(message)s',
    handlers=[stdout_handler, stderr_handler]
)
logger = logging.getLogger(__name__)


class ASCIIGridHandler:
    """Handle reading and writing of ESRI ASCII Grid files."""
    
    @staticmethod
    def read_ascii_grid(filepath: str) -> Tuple[np.ndarray, Dict[str, Any]]:
        """
        Read ESRI ASCII grid file (.asc) and return data array and metadata.
        
        Returns:
            tuple: (data_array, metadata_dict)
        """
        try:
            with open(filepath, 'r') as f:
                lines = f.readlines()
            
            # Parse header
            header = {}
            data_start_line = 0
            
            for i, line in enumerate(lines):
                line = line.strip().lower()
                if line.startswith('ncols'):
                    header['ncols'] = int(line.split()[1])
                elif line.startswith('nrows'):
                    header['nrows'] = int(line.split()[1])
                elif line.startswith('xllcorner') or line.startswith('xllcenter'):
                    header['xllcorner'] = float(line.split()[1])
                elif line.startswith('yllcorner') or line.startswith('yllcenter'):
                    header['yllcorner'] = float(line.split()[1])
                elif line.startswith('cellsize'):
                    header['cellsize'] = float(line.split()[1])
                elif line.startswith('nodata_value'):
                    header['nodata_value'] = float(line.split()[1])
                else:
                    # First line that doesn't match header format
                    data_start_line = i
                    break
            
            # Set default nodata value if not specified
            if 'nodata_value' not in header:
                header['nodata_value'] = -9999.0
            
            # Read data
            data_lines = lines[data_start_line:]
            data = []
            
            for line in data_lines:
                if line.strip():  # Skip empty lines
                    row = [float(x) for x in line.strip().split()]
                    data.append(row)
            
            # Convert to numpy array
            data_array = np.array(data, dtype=np.float64)
            
            # Validate dimensions
            if data_array.shape != (header['nrows'], header['ncols']):
                raise ValueError(f"Data dimensions {data_array.shape} don't match header "
                               f"({header['nrows']}, {header['ncols']})")
            
            logger.info(f"Loaded ASCII grid: {data_array.shape} cells, "
                       f"cellsize: {header['cellsize']}")
            
            return data_array, header
            
        except Exception as e:
            logger.error(f"Failed to read ASCII grid {filepath}: {e}")
            raise
    
    @staticmethod
    def write_ascii_grid(filepath: str, data: np.ndarray, header: Dict[str, Any]) -> None:
        """
        Write numpy array to ESRI ASCII grid file (.asc).
        
        Args:
            filepath: Output file path
            data: 2D numpy array
            header: Dictionary with grid metadata
        """
        try:
            with open(filepath, 'w') as f:
                # Write header
                f.write(f"ncols         {header['ncols']}\n")
                f.write(f"nrows         {header['nrows']}\n")
                f.write(f"xllcorner     {header['xllcorner']:.6f}\n")
                f.write(f"yllcorner     {header['yllcorner']:.6f}\n")
                f.write(f"cellsize      {header['cellsize']:.6f}\n")
                f.write(f"NODATA_value  {header['nodata_value']:.6f}\n")
                
                # Write data
                for row in data:
                    row_str = ' '.join(f'{val:.6f}' for val in row)
                    f.write(row_str + '\n')
            
            logger.info(f"Saved ASCII grid: {filepath}")
            
        except Exception as e:
            logger.error(f"Failed to write ASCII grid {filepath}: {e}")
            raise


class DEMProcessor:
    """Class to handle DEM processing operations."""
    
    def __init__(self):
        self.dem_data = None
        self.header = None
        self.cellsize = None
        self.nodata_value = None
    
    def load_dem(self, dem_path: str) -> None:
        """Load DEM file and extract metadata."""
        self.dem_data, self.header = ASCIIGridHandler.read_ascii_grid(dem_path)
        self.cellsize = self.header['cellsize']
        self.nodata_value = self.header['nodata_value']
        
        # Create mask for valid data (not nodata)
        self.valid_mask = self.dem_data != self.nodata_value
        
        logger.info(f"DEM loaded: {self.dem_data.shape} pixels, "
                   f"cellsize: {self.cellsize}, "
                   f"valid cells: {np.sum(self.valid_mask)}/{self.dem_data.size}")
    
    def compute_gradients(self) -> Tuple[np.ndarray, np.ndarray]:
        """Compute elevation gradients in X and Y directions using finite differences."""
        rows, cols = self.dem_data.shape
        
        # Initialize gradient arrays
        grad_x = np.full_like(self.dem_data, self.nodata_value)
        grad_y = np.full_like(self.dem_data, self.nodata_value)
        
        # Calculate X gradient (east-west)
        # Interior points: central difference
        grad_x[:, 1:-1] = (self.dem_data[:, 2:] - self.dem_data[:, :-2]) / (2 * self.cellsize)
        # Boundary points: forward/backward difference
        grad_x[:, 0] = (self.dem_data[:, 1] - self.dem_data[:, 0]) / self.cellsize
        grad_x[:, -1] = (self.dem_data[:, -1] - self.dem_data[:, -2]) / self.cellsize
        
        # Calculate Y gradient (north-south)
        # Note: Y increases downward in grid, but elevation increases upward
        # Interior points: central difference
        grad_y[1:-1, :] = (self.dem_data[:-2, :] - self.dem_data[2:, :]) / (2 * self.cellsize)
        # Boundary points: forward/backward difference
        grad_y[0, :] = (self.dem_data[0, :] - self.dem_data[1, :]) / self.cellsize
        grad_y[-1, :] = (self.dem_data[-2, :] - self.dem_data[-1, :]) / self.cellsize
        
        # Set nodata where original DEM has nodata
        grad_x[~self.valid_mask] = self.nodata_value
        grad_y[~self.valid_mask] = self.nodata_value
        
        return grad_x, grad_y
    
    def calculate_slope(self, grad_x: np.ndarray, grad_y: np.ndarray) -> np.ndarray:
        """Calculate slope in degrees from gradients."""
        # Initialize output with nodata values
        slope = np.full_like(self.dem_data, self.nodata_value)
        
        # Calculate slope only for valid data
        valid = (grad_x != self.nodata_value) & (grad_y != self.nodata_value)
        
        if np.any(valid):
            slope_radians = np.arctan(np.sqrt(grad_x[valid]**2 + grad_y[valid]**2))
            slope[valid] = np.degrees(slope_radians)
        
        return slope
    
    def calculate_aspect(self, grad_x: np.ndarray, grad_y: np.ndarray) -> np.ndarray:
        """Calculate aspect in degrees from gradients."""
        # Initialize output with nodata values
        aspect = np.full_like(self.dem_data, self.nodata_value)
        
        # Calculate aspect only for valid data
        valid = (grad_x != self.nodata_value) & (grad_y != self.nodata_value)
        
        if np.any(valid):
            # Calculate aspect using atan2 (handles all quadrants)
            aspect_radians = np.arctan2(grad_y[valid], -grad_x[valid])
            aspect_degrees = np.degrees(aspect_radians)
            
            # Convert to compass bearing (0-360 degrees, 0=North, 90=East)
            aspect_degrees = (90 - aspect_degrees) % 360
            
            # Handle nearly flat areas (slope < 0.1 degrees)
            slope_radians = np.arctan(np.sqrt(grad_x[valid]**2 + grad_y[valid]**2))
            flat_mask = np.degrees(slope_radians) < 0.1
            aspect_degrees[flat_mask] = -1
            
            aspect[valid] = aspect_degrees
        
        return aspect
    
    def calculate_shaded_relief(self, grad_x: np.ndarray, grad_y: np.ndarray, 
                              sun_azimuth: float, sun_altitude: float = 45.0) -> np.ndarray:
        """Calculate shaded relief using Lambertian reflectance model."""
        # Initialize output with nodata values
        hillshade = np.full_like(self.dem_data, self.nodata_value)
        
        # Calculate hillshade only for valid data
        valid = (grad_x != self.nodata_value) & (grad_y != self.nodata_value)
        
        if np.any(valid):
            # Convert sun angles to radians
            azimuth_rad = np.radians(sun_azimuth)
            altitude_rad = np.radians(sun_altitude)
            
            # Calculate slope and aspect in radians
            slope_rad = np.arctan(np.sqrt(grad_x[valid]**2 + grad_y[valid]**2))
            aspect_rad = np.arctan2(grad_y[valid], -grad_x[valid])
            
            # Hillshade calculation using dot product of surface normal and light vector
            hillshade_values = (np.cos(altitude_rad) * np.cos(slope_rad) +
                              np.sin(altitude_rad) * np.sin(slope_rad) *
                              np.cos(azimuth_rad - aspect_rad))
            
            # Normalize to 0-255 range
            hillshade_values = np.clip(hillshade_values * 255, 0, 255)
            hillshade[valid] = hillshade_values
        
        return hillshade
    
    def save_result(self, data: np.ndarray, output_path: str) -> None:
        """Save result using the same header as input DEM."""
        ASCIIGridHandler.write_ascii_grid(output_path, data, self.header)


def load_configuration() -> Tuple[str, str, str]:
    """Load file paths from environment variables."""
    try:
        # Try to use decouple if available
        try:
            from decouple import config
            input_artifacts = config('INPUT_ARTIFACTS_PATH')
            input_parameters = config('INPUT_PARAMETERS_PATH')
            output_artifacts = config('OUTPUT_ARTIFACTS_PATH')
        except ImportError:
            # Fallback to os.environ if decouple not available
            logger.info("decouple not available, using os.environ")
            input_artifacts = os.environ['INPUT_ARTIFACTS_PATH']
            input_parameters = os.environ['INPUT_PARAMETERS_PATH']
            output_artifacts = os.environ['OUTPUT_ARTIFACTS_PATH']
        
        # Validate paths exist
        for path_name, path_value in [
            ('INPUT_ARTIFACTS_PATH', input_artifacts),
            ('INPUT_PARAMETERS_PATH', input_parameters)
        ]:
            if not os.path.exists(path_value):
                raise FileNotFoundError(f"{path_name} does not exist: {path_value}")
        
        return input_artifacts, input_parameters, output_artifacts
        
    except KeyError as e:
        logger.error(f"Missing environment variable: {e}")
        raise
    except Exception as e:
        logger.error(f"Configuration error: {e}")
        raise


def read_azimuth_parameter(parameters_path: str) -> float:
    """Read sun azimuth angle from parameters file."""
    azimuth_file = os.path.join(parameters_path, "azimuth")
    
    try:
        with open(azimuth_file, 'r') as f:
            azimuth = float(f.read().strip())
            
        if not 0 <= azimuth <= 360:
            logger.warning(f"Azimuth {azimuth} outside expected range [0-360]")
            
        logger.info(f"Sun azimuth: {azimuth} degrees")
        return azimuth
        
    except (FileNotFoundError, ValueError) as e:
        logger.error(f"Failed to read azimuth from {azimuth_file}: {e}")
        raise


def main():
    """Main processing function."""
    try:
        logger.info("Starting DEM terrain analysis (no external dependencies)...")
        
        # Load configuration
        input_artifacts_path, input_parameters_path, output_artifacts_path = load_configuration()
        
        # Define file paths
        dem_file = os.path.join(input_artifacts_path, "dem.asc")
        slope_output = os.path.join(output_artifacts_path, "slope.asc")
        aspect_output = os.path.join(output_artifacts_path, "aspect.asc")
        relief_output = os.path.join(output_artifacts_path, "relief.asc")
        
        # Read parameters
        sun_azimuth = read_azimuth_parameter(input_parameters_path)
        
        # Create output directory if it doesn't exist
        os.makedirs(output_artifacts_path, exist_ok=True)
        
        # Initialize processor and load DEM
        processor = DEMProcessor()
        processor.load_dem(dem_file)
        
        # Calculate gradients
        logger.info("Computing elevation gradients...")
        grad_x, grad_y = processor.compute_gradients()
        
        # Calculate terrain parameters
        logger.info("Calculating slope...")
        slope = processor.calculate_slope(grad_x, grad_y)
        
        logger.info("Calculating aspect...")
        aspect = processor.calculate_aspect(grad_x, grad_y)
        
        logger.info("Calculating shaded relief...")
        relief = processor.calculate_shaded_relief(grad_x, grad_y, sun_azimuth)
        
        # Save results
        logger.info("Saving results...")
        processor.save_result(slope, slope_output)
        processor.save_result(aspect, aspect_output)
        processor.save_result(relief, relief_output)
        
        logger.info("DEM terrain analysis completed successfully!")
        
        # Print summary statistics (only for valid data)
        valid_slope = slope[slope != processor.nodata_value]
        valid_aspect = aspect[aspect != processor.nodata_value]
        valid_relief = relief[relief != processor.nodata_value]
        
        if len(valid_slope) > 0:
            logger.info(f"Slope range: {valid_slope.min():.2f} - {valid_slope.max():.2f} degrees")
        if len(valid_aspect) > 0:
            logger.info(f"Aspect range: {valid_aspect.min():.2f} - {valid_aspect.max():.2f} degrees")
        if len(valid_relief) > 0:
            logger.info(f"Relief range: {valid_relief.min():.2f} - {valid_relief.max():.2f}")
        
    except Exception as e:
        logger.error(f"Processing failed: {e}")
        sys.exit(1)


if __name__ == "__main__":
    main()