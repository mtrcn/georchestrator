# GeOrchestrator

![Logo](logo.png)

This project was developed as a part of PhD study. This project allows running 
geospatial workflows on serverless services provided by a cloud provider. It supports 
sequential, parallel, and loop executions. In addition, a new workflow and task definition 
models were introduced to design workflows in a simple, versionable, and readable way. 

## Usage

At the moment, there is only AWS deployment code. Other platforms are welcomed as pull requests.

### AWS Deployment

Under `deployments` folder, run following script;

```powershell
./Deploy.ps1
```

This will create two lambda functions for APIs, DynamoDB tables (on-demand), Fargate cluster with required network components. 

## Contributing
Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

Please make sure to update tests as appropriate.

## Publications

Pakdil, M.E.; Çelik, R.N. Serverless Geospatial Data Processing Workflow System Design. 
ISPRS Int. J. Geo-Inf. 2022, 11, 20. https://doi.org/10.3390/ijgi11010020

## License
[Apache License 2.0](https://choosealicense.com/licenses/apache-2.0/)

[![DOI](https://zenodo.org/badge/325787241.svg)](https://zenodo.org/badge/latestdoi/325787241)