# UploadImageDotNetCore
Example .Net Core project to upload image with basic validation.

- Simple web interface to upload files
- Upload images to `upload` directory
- Limit the number of files uploaded to the saving directory ( 10 files )
- Validate image file using file signatures
- Limit minimum and maximum file size ( 0 B < size < 1 GB )

## Commands
Start
```
dotnet run
```

Build
```
dotnet publish -o ./build -c Production
```