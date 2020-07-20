# roster-api-lambda

## Setup

* [AWS Toolkit for Visual Studio 2017 and 2019](https://marketplace.visualstudio.com/items?itemName=AmazonWebServices.AWSToolkitforVisualStudio2017)

Install Amazon.Lambda.Tools Global Tools if not already installed.
```
    dotnet tool install -g Amazon.Lambda.Tools
```

If already installed check if new version is available.
```
    dotnet tool update -g Amazon.Lambda.Tools
```
## Deployment

### Build:  To build the .NET Core AWS Lambda function
```
    dotnet build
```

### Package:  To package a .NET Core AWS Lambda function and supporting files
```
    dotnet lambda package
```

### Deploy:  To update .NET Core AWS Lambda function
```
    dotnet lambda deploy-function --profile roster-admin-user --function-name roster-api-lambda --package C:\JPFiles\src\roster-api-lambda\bin\Debug\netcoreapp3.1\roster-api-lambda.zip
```

### This .bat file will run the build + package + deploy commands 
```
    ./run-all.bat
```

### To publish, open a `Package Manager Console` window and run the `publish.ps1` script:
```
    ./publish
```

----

## PDF

* [Why do I get an error saying that "use of extended features is no longer available"?](https://itextpdf.com/en/resources/faq/technical-support/itext-5-legacy/why-do-i-get-error-saying-use-extended-features-no)

----

## See also

* [ROSTER on GitHub](https://github.com/jaypalexa/roster)
