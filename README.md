# Dynamic Dependency Injections [![Build status](https://ci.appveyor.com/api/projects/status/67ubhtmijuhyhq6q?svg=true)](https://ci.appveyor.com/project/eshohag/eShohag.AspNetCore.DynamicInjection) [![NuGet Badge](https://buildstats.info/nuget/eShohag.AspNetCore.DynamicInjection)](https://www.nuget.org/packages/eShohag.AspNetCore.DynamicInjection) [![NuGet Badge](https://buildstats.info/nuget/AspNetCore.DynamicInjection)](https://www.nuget.org/packages/AspNetCore.DynamicInjection)
Default implementation of dynamic dependency injections in ASP.NET Core project

### Namespace
    using Microsoft.Extensions.DependencyInjection.DynamicInjection

### Code Syntax

  - If your project is single
  
         public void ConfigureServices(IServiceCollection services)
         {
            services.AddServicesOfType<IScopedService>();
            services.AddServicesAttributeOfType<ScopedServiceAttribute>();      
         }
         
- If your project is multiple

        public void ConfigureServices(IServiceCollection services)
        {
            //Assemblies start with "Demo.DynamicInjections", "Demo.Repository", "Demo.Manager" will only be scanned.
            string[] assembliesToBeScanned = new string[] { "Demo.DynamicInjections", "Demo.Manager", "Demo.Repository" };
            services.AddServicesOfType<IScopedService>(assembliesToBeScanned);
            services.AddServicesAttributeOfType<ScopedServiceAttribute>(assembliesToBeScanned);
        }
        
- Every interface can hold attribute- Scoped Service
    
      [ScopedService]
      public interface IStudentService
      {
          List<Student> GetStudents();
      }
      
- As usual calling code

      public class StudentController : Controller
      {
          private readonly IStudentService _studentService;

          public StudentController(IStudentService studentService)
          {
              _studentService = studentService;
          }

          public IActionResult Index()
          {
              var studentsByService = _studentService.GetStudents();

              return View();
          }
      }
