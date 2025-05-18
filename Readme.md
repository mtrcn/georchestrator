# GeOrchestrator

![Logo](logo.png)

This project was developed as a part of PhD study. This project allows running 
geospatial workflows on serverless services provided by a cloud provider. It supports 
sequential, parallel, and loop executions. In addition, a new workflow and task definition 
models were introduced to design workflows in a simple, versionable, and readable way. 

## Usage

At the moment, it only works on AWS and local docker deployment. Other platforms are welcomed as pull requests.

### AWS Deployment

Under `./deployments/GEOrchestrator.Aws.Deployment` folder, run following script;

```powershell
./Deploy.ps1
```

This will create two lambda functions for APIs, DynamoDB tables (on-demand), Fargate cluster with required network components. 

### Docker Deployment

Under the root folder create a new `.env` file with the content as in the example below;

```
AWS_REGION = [Your AWS Region, eg. eu-west-1]
AWS_ACCESS_KEY_ID = [Your AWS Access Key]
AWS_SECRET_ACCESS_KEY = [Your AWS Secret Key]
ASPNETCORE_ENVIRONMENT = Development
OBJECT_REPOSITORY_PROVIDER = s3
AWS_S3_BUCKET_NAME = georchestrator-objects
PARAMETER_REPOSITORY_PROVIDER = dynamodb
ARTIFACT_REPOSITORY_PROVIDER = dynamodb
WORKFLOW_REPOSITORY_PROVIDER = dynamodb
JOB_REPOSITORY_PROVIDER = dynamodb
TASK_REPOSITORY_PROVIDER = dynamodb
EXECUTION_STEP_REPOSITORY_PROVIDER = dynamodb
EXECUTION_STEP_MESSAGE_REPOSITORY_PROVIDER = dynamodb
EXECUTION_REPOSITORY_PROVIDER = dynamodb
WORKFLOW_API_URL = http://host.docker.internal:8000
CONTAINER_PROVIDER = docker
FARGATE_REGION = 
FARGATE_EXECUTION_ROLE_ARN = 
FARGATE_CLUSTER_NAME = 
FARGATE_SUBNET_ID = 
FARGATE_SECURITY_GROUP_ID = 
```

then run the following command;

```bash
docker compose up
```

## Contributing
Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

Please make sure to update tests as appropriate.

## Publications

Pakdil, M.E.; Celik, R.N. Serverless Geospatial Data Processing Workflow System Design. 
ISPRS Int. J. Geo-Inf. 2022, 11, 20. https://doi.org/10.3390/ijgi11010020

## License
[Apache License 2.0](https://choosealicense.com/licenses/apache-2.0/)

[![DOI](https://zenodo.org/badge/325787241.svg)](https://zenodo.org/badge/latestdoi/325787241)