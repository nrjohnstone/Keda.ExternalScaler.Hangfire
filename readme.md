# KEDA External Scaler - Hangfire

## Overview

This repository is an example implementation of the KEDA External Scaler specification written in C# / .NET Core 3.x

It demonstates how to build a GRPC service that will attach to any number of named Hangfire instances and provide scaling metrics to KEDA based on the number of enqueued jobs.

The code in this repository assumes a number of things about you

* You know how to use kubectl to deploy things to Kubernetes
* You know how to use a container registry to host your own images
* You use Microsoft Azure (should be easy to modify for other cloud environments)
* You know how hangfire works, or at least are familiar with producer/consumer patterns using queues as the
medium for requesting work from consumers

## Prerequisites

### YTT - Yaml Templating Tool
This is used to template out the Kubernetes yaml files.
Get it from here https://get-ytt.io/

### Container Registry
You need to have a container registry that you can push the containers from this repository to and your kubernetes can deploy from it.

## Getting Started

### Setup Configuration Files
You will need to make copies of the values.yaml.template and environment.ps1.template files without the .template extension and add your
container registry address and the IP address of an external load balancer that can be assigned to the SQL server deployment.

Once you have this you will need to run the environment.ps1 file to add the environment variable to your current Powershell session.

### Build Code
This repository uses Cake as a build script language.

You can build and push all the containers to your container registry by using the following from the Powershell terminal

.\build.ps1 -target Docker-Push-All

### Deploy To Kubernetes

All of the deployment yaml files use the templating tool "ytt" to move environment specific values into optional files.

```
ytt -f .\keda-scaled-job-multiply.yaml -f .\values.yaml | kubectl apply -f-
ytt -f .\keda-scaled-job-addition.yaml -f .\values.yaml | kubectl apply -f-
ytt -f .\sql-server-deployment.yaml -f .\values.yaml | kubectl apply -f- 
ytt -f .\keda-externalscaler-hangfire.yaml -f .\values.yaml | kubectl apply -f-
ytt -f .\hangfire-dashboard.yaml -f .\values.yaml | kubectl apply -f-
```

These are all included in the powershell script ```deploy.ps1```

### Running Locally
The Hangfire.Dashboard, Hangfire.Producer and Hangfire.Consumer applications can all be run locally for testing etc... but before doing so you have
to configure the settings files correctly.

To ensure secrets do not get checking into the git repository, the applications will always check under users local app data folder for a folder called
"Keda.ExternalScaler.Hangfire" and if there is a settings.yaml in this folder, it will include this, overriding values from the default settings.yaml in the 
application root.

The "Common.Utilities" assembly has the required information to how the settings work, in the ApplicationConfiguration.cs file.

### Hangfire Dashboard
The Hangfire.Dashboard project is a simple .NET Core WebApi that hosts the Hangfire Dashboard. This can be used to browse queues to see 
what the status of various jobs are, requeue jobs, delete jobs etc...

Currently the ingress rule is using the hostname "hangfire-dashboard.internal" which you can add to your localhosts file and point to your
relevant load balancer address.

Or you can run it locally from your machine in docker, or just run the executable directly.
