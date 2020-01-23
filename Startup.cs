using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
// to be used by Swashbuckle v5
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;

namespace WebAPI.NetCore
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Adding Cors ---- Multiple Policy
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(
                    builder =>
                    builder.AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    //.WithExposedHeaders("content-disposition")
                    //.DisallowCredentials()
                    //.SetIsOriginAllowedToAllowWildcardSubdomains()
                    //.AllowCredentials()
                    //.SetPreflightMaxAge(TimeSpan.FromSeconds(3600))
                    );

                options.AddPolicy("CorsPolicy",
                    builder =>
                    {
                        builder.WithOrigins(
                            "http://datacollection.milad.aga.my",
                            "http://localhost:3000")
                                .AllowAnyHeader()
                                .AllowAnyMethod()
                                .AllowCredentials()
                                .SetIsOriginAllowedToAllowWildcardSubdomains();
                    });
            });
            //services.AddCors(c =>
            //{
            //    c.AddPolicy("AllowOrigin", options => options.WithOrigins("https://localhost:3000"));
            //});

            // Trying signalR
            services.AddSignalR();

            services.AddAuthorization(options => 
            {
                options.DefaultPolicy = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme).RequireAuthenticatedUser().Build();
            });

            services.AddAuthentication(o =>
            {
                o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(cfg =>
            {
                cfg.RequireHttpsMetadata = false;
                cfg.SaveToken = true;

                cfg.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidIssuer = Configuration["Jwt:Issuer"],//"Tokens:Issuer",
                    ValidAudience = Configuration["Jwt:Issuer"],//"Tokens:Issuer",
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:Key"]))//"mysupers3cr3tsharedkey!"
                };

            }).AddCookie(cfg => cfg.SlidingExpiration = true);

            services.AddMvc(options => 
            {
                options.EnableEndpointRouting = false; // .net core 3
                options.RespectBrowserAcceptHeader = true;
                options.OutputFormatters.Add(new Microsoft.AspNetCore.Mvc.Formatters.XmlSerializerOutputFormatter());
                //options.OutputFormatters.RemoveType<Microsoft.AspNetCore.Mvc.Formatters.TextOutputFormatter>();
                //options.OutputFormatters.RemoveType<Microsoft.AspNetCore.Mvc.Formatters.HttpNoContentOutputFormatter>();

            })
                .AddNewtonsoftJson(options =>
                            options.SerializerSettings.ContractResolver =
                                new CamelCasePropertyNamesContractResolver()); // .net core 3 Migration

            // .net Core 3 Migration
            services.AddControllers().AddNewtonsoftJson(options =>
                   options.SerializerSettings.ContractResolver =
                      new CamelCasePropertyNamesContractResolver());
            // Adding Swagger
            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v3", new OpenApiInfo
                {
                    Version = "v3",
                    Title = "Object Detection API",
                    Description = ".NET Core Web API",
                    //TermsOfService = new Uri("None"),
                    Contact = new OpenApiContact
                    {
                        Name = "Developers",
                        Email = "Mohsen@aga.my",
                        Url = new Uri("https://www.agagroup.my/")
                    },
                    License = new OpenApiLicense
                    {
                        Name = "Use under LICX",
                        Url = new Uri("https://example.com/license")
                    }
                });

                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath, true);
            });

            //services.ConfigureSwaggerGen(options =>
            //{
            //    options.OperationFilter<AuthorizationHeaderParameterOperationFilter>();
            //    options.OperationFilter<FileUploadOperation>(); //Register File Upload Operation Filter
            //    options.SchemaFilter<SwaggerExcludeSchemaFilter>(); // Exclude some of the property fields from Swagger UI
            //});

            // Binding appsetting.json JWT section to the IOption (to be accessed in Controllers)
            services.Configure<Models.Jwt>(Configuration.GetSection("Jwt"));

            // Directory Browsing
            services.AddDirectoryBrowser();

            // IIS Options
            services.Configure<IISOptions>(options =>
            {
                options.ForwardClientCertificate = false;
                options.AutomaticAuthentication = true;
            });

            // *** Dependency Injection:: Registering the IUserService with the Dependency Injection system ****
            // "Singleton" which creates a single instance throughout the application.It creates the instance for the first time and reuses the same object in the all calls.
            // "Scoped" lifetime services are created once per request within the scope.It is equivalent to Singleton in the current scope.eg. in MVC it creates 1 instance per each http request but uses the same instance in the other calls within the same web request.
            // "Transient" lifetime services are created each time they are requested.This lifetime works best for lightweight, stateless services.
            services.AddScoped<Interfaces.IUserService, Services.UserService>();
        }
        // To Apply Authentication Filter to Swagger UI
        //public class AuthorizationHeaderParameterOperationFilter : IOperationFilter
        //{
        //    public void Apply(Operation operation, OperationFilterContext context)
        //    {
        //        var filterPipeline = context.ApiDescription.ActionDescriptor.FilterDescriptors;
        //        var isAuthorized = filterPipeline.Select(filterInfo => filterInfo.Filter).Any(filter => filter is AuthorizeFilter);
        //        var allowAnonymous = filterPipeline.Select(filterInfo => filterInfo.Filter).Any(filter => filter is IAllowAnonymousFilter);

        //        if (isAuthorized && !allowAnonymous)
        //        {
        //            if (operation.Parameters == null)
        //                operation.Parameters = new List<IParameter>();

        //            operation.Parameters.Add(new NonBodyParameter
        //            {
        //                Name = "Authorization",
        //                In = "header",
        //                Description = "token",
        //                Required = true,
        //                Type = "string"
        //            });
        //        }
        //    }
        //}
        //// Exclude some properties
        //public class SwaggerExcludeSchemaFilter : ISchemaFilter
        //{
        //    public void Apply(Schema schema, SchemaFilterContext context)
        //    {
        //        if (schema?.Properties == null)
        //        {
        //            return;
        //        }

        //        var excludedProperties =
        //            context.SystemType.GetProperties().Where(
        //                t => t.GetCustomAttribute<SwaggerExcludeAttribute>() != null);

        //        foreach (var excludedProperty in excludedProperties)
        //        {
        //            var propertyToRemove =
        //                schema.Properties.Keys.SingleOrDefault(
        //                    x => x.ToLower() == excludedProperty.Name.ToLower());

        //            if (propertyToRemove != null)
        //            {
        //                schema.Properties.Remove(propertyToRemove);
        //            }
        //        }
        //    }
        //}
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseCors("CorsPolicy");
            //app.UseCors();
            // Trying webSocket
            app.UseWebSockets();

            // Trying SignalR
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                //endpoints.MapRazorPages();
                endpoints.MapControllers();
                endpoints.MapHub<Core.ClientHub>("/clientHub");
            });
            //app.UseCors(options => options.WithOrigins("https://localhost:3000"));
            app.UseMvc();
            // ******* Notice :: Access Static content inside wwwroot Folder!!!!!
            app.UseStaticFiles();
            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();
            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), 
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v3/swagger.json", "Core API V3");
                // To serve the Swagger UI at the app's root (http://localhost:<port>/)
                c.RoutePrefix = string.Empty;
            });

            // Directory Browsing
            app.UseDirectoryBrowser(new DirectoryBrowserOptions
            {
                FileProvider = new PhysicalFileProvider(
                Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Processor/uploads")), //env.WebRootPath, "Processor", "uploads" || 
                RequestPath = "/Processor/uploads"
            });
            app.UseDirectoryBrowser(new DirectoryBrowserOptions
            {
                FileProvider = new PhysicalFileProvider(
                Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Processor/cilidetection")),
                RequestPath = "/Processor/cilidetection"
            });
        }
    }
}
