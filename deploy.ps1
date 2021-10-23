ytt -f .\kubernetes\namespace.yaml -f .\kubernetes\values.yaml | kubectl apply -f- 
ytt -f .\kubernetes\keda-scaled-job-multiply.yaml -f .\kubernetes\values.yaml | kubectl apply -f-
ytt -f .\kubernetes\keda-scaled-job-addition.yaml -f .\kubernetes\values.yaml | kubectl apply -f-
ytt -f .\kubernetes\sql-server-deployment.yaml -f .\kubernetes\values.yaml | kubectl apply -f- 
ytt -f .\kubernetes\keda-externalscaler-hangfire.yaml -f .\kubernetes\values.yaml | kubectl apply -f-
ytt -f .\kubernetes\hangfire-dashboard.yaml -f .\kubernetes\values.yaml | kubectl apply -f-