using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using WebApiAutores.Filtros;
using WebApiAutores.Middlewares;
using WebApiAutores.Servicios;
using WebApiAutores.Utilidades;
[assembly:ApiConventionType(typeof(DefaultApiConventions))]
namespace WebApiAutores
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers(optiones =>
            {
                optiones.Filters.Add(typeof(FiltroDeExcepcion));
                optiones.Conventions.Add(new SwaggerAgrupaPorVersion());
            }).AddJsonOptions(x=>x.JsonSerializerOptions.ReferenceHandler=ReferenceHandler.IgnoreCycles).AddNewtonsoftJson();
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("defaultConnection"));
            });
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "WebApiAutores", Version = "v1" });
                c.SwaggerDoc("v2", new OpenApiInfo { Title = "WebApiAutores", Version = "v2" });
                c.OperationFilter<AgregarParametroHATEOAS>();
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference=new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id="Bearer"
                            }
                        },
                        new string[]{}
                    }
                });
                var archivoXML = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var rutaXML = Path.Combine(AppContext.BaseDirectory, archivoXML);
                c.IncludeXmlComments(rutaXML);
            });
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options => options.TokenValidationParameters = new TokenValidationParameters()
            {
                ValidateIssuer=false,
                ValidateAudience=false,
                ValidateLifetime=true,
                ValidateIssuerSigningKey=true,
                IssuerSigningKey=new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["llavejwt"])),
                ClockSkew=TimeSpan.Zero
            });
            services.AddAutoMapper(typeof(Startup));
            services.AddIdentity<IdentityUser, IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();
            services.AddAuthorization(opciones =>
            {
                opciones.AddPolicy("EsAdmin", politica => politica.RequireClaim("esAdmin"));
            });
            services.AddCors(opciones =>
            {
                opciones.AddDefaultPolicy(builder =>
                {
                    builder.WithOrigins("https://www.apirequest.io").AllowAnyMethod().AllowAnyHeader().WithExposedHeaders(new string[] {"cantidadTotalRegistros"});
                    
                });
            });
            services.AddDataProtection();
            services.AddTransient<HashService>();
            services.AddTransient<GeneradorEnlaces>();
            services.AddTransient<HATEOASAutorFilterAttribute>();
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
        }
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env,ILogger<Startup> logger)
        {
            app.UseLoguearRespuestaHTTPMiddleware();          
            if (env.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebApiAutores V1");
                    c.SwaggerEndpoint("/swagger/v2/swagger.json", "WebApiAutores V2");
                });
                app.UseDeveloperExceptionPage();
            }
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseCors();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
