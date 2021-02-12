# Dynamic Dependency Injections in ASP.NET Core Information
[![Build status](https://ci.appveyor.com/api/projects/status/67ubhtmijuhyhq6q?svg=true)](https://ci.appveyor.com/project/eshohag/eShohag.AspNetCore.DynamicInjection)
[![NuGet Badge](https://buildstats.info/nuget/eShohag.AspNetCore.DynamicInjection)](https://www.nuget.org/packages/eShohag.AspNetCore.DynamicInjection)

# Code Syntax

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
          Student GetStudent();
      }
      
- As usual calling code

      public class StudentController : Controller
      {
          private readonly IStudentService _studentService;
          private readonly IStudentManager _studentManager;

          public StudentController(IStudentService studentService, IStudentManager studentManager)
          {
              _studentService = studentService;
              _studentManager = studentManager;
          }

          public IActionResult Index()
          {
              var studentsByService = _studentService.GetStudents();
              var studentsByManager = _studentManager.GetStudents();

              return View();
          }
      }
