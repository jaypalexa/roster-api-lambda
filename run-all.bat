dotnet build
dotnet lambda package
dotnet lambda deploy-function --profile roster-admin-user --function-name roster-api-lambda --package C:\JPFiles\src\roster-api-lambda\bin\Debug\netcoreapp3.1\roster-api-lambda.zip
