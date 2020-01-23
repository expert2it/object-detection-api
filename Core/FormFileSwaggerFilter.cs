using Microsoft.AspNetCore.Http;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace WebAPI.NetCore
{
    /// <summary>
    /// Filter to enable handling file upload in swagger
    /// </summary>
    //public class FileUploadOperation : IOperationFilter
    //{
    //    public void Apply(Operation operation, OperationFilterContext context)
    //    {
    //        // Swagger v3    
    //        //if (operation.OperationId.ToLower() == "detectionobjectdetectionpost" || operation.OperationId.ToLower() == "detectiontextrecognitionpost")
    //        // Swagger v4
    //        if (operation.OperationId.ToLower() == "cilidetection" || operation.OperationId.ToLower() == "objectdetection" || operation.OperationId.ToLower() == "textrecognition" || operation.OperationId.ToLower() == "uploadfiles")
    //        {
    //            //operation.Parameters.Clear();
    //            var files = operation.Parameters[0];
    //            operation.Parameters.Remove(files);
    //            operation.Parameters.Add(new NonBodyParameter
    //            {
    //                Name = "files",
    //                In = "formData",
    //                Description = "Upload File",
    //                Required = true,
    //                Type = "file"
    //            });
    //            operation.Consumes.Add("multipart/form-data");
    //        }
    //    }
    //}
}
