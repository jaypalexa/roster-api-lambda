# Copy-Item -Path "pdf/*.pdf" -Destination "."
dotnet lambda deploy-function --profile roster-admin-user --function-name roster-api-lambda
# Remove-Item -Path "*.pdf"
