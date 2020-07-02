ytt -f .\keda-scaled-job-multiply.yaml -f .\values.yaml | kubectl apply -f-
ytt -f .\keda-scaled-job-addition.yaml -f .\values.yaml | kubectl apply -f-
ytt -f .\sql-server-deployment.yaml -f .\values.yaml | kubectl apply -f- 
ytt -f .\keda-externalscaler-hangfire.yaml -f .\values.yaml | kubectl apply -f-
ytt -f .\hangfire-dashboard.yaml -f .\values.yaml | kubectl apply -f-