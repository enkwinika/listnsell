# ?? COMPLETE TROUBLESHOOTING GUIDE - REXELL MARKETPLACE

## ?? **Quick Problem Solver**

Choose your issue:

| Issue | Solution Document | Time |
|-------|------------------|------|
| ? **IntelliSense errors (red squiggles)** | `FIX_RAZOR_VIEW_ERRORS.md` | 1-2 min |
| ? **Newtonsoft.Json assembly error** | `FIX_NEWTONSOFT_JSON_ERROR.md` | 1 min |
| ? **Build errors** | `FIX_BUILD_ERRORS.md` | 5 min |
| ? **NuGet package errors** | `UPDATE_SECURITY_PACKAGES.txt` | 10 min |
| ? **Ready to start UI** | `BUILD_STATUS_AND_NEXT_STEPS.md` | - |
| ? **Need UI quick start** | `UI_QUICK_START.md` | 15 min |

---

## ?? **ISSUE 1: IntelliSense Errors (Red Squiggles)**

### Symptoms:
```
? System.Linq does not exist
? DynamicAttribute cannot be found
?? Build: Succeeded
```

### Cause:
**Visual Studio IntelliSense cache is outdated**

### Fix:
**Quick Fix (1 minute):**
1. Save all files (Ctrl+Shift+S)
2. Close Visual Studio
3. Reopen solution
4. Rebuild (Ctrl+Shift+B)

**Full Guide:** See `FIX_RAZOR_VIEW_ERRORS.md`

---

## ?? **ISSUE 2: Newtonsoft.Json Assembly Error**

### Symptoms:
```
? Could not load file or assembly 'Newtonsoft.Json'
? HRESULT: 0x80131040
```

### Cause:
**Assembly binding redirect version mismatch**

### Fix:
**? ALREADY FIXED!**

Web.config updated to:
```xml
<bindingRedirect oldVersion="0.0.0.0-13.0.0.0" newVersion="13.0.3" />
```

**Steps:**
1. Clean Solution (Build ? Clean)
2. Rebuild (Ctrl+Shift+B)
3. Run (F5)

**Full Guide:** See `FIX_NEWTONSOFT_JSON_ERROR.md`

---

## ?? **ISSUE 3: Other Assembly Binding Errors**

### Symptoms:
```
? Could not load file or assembly 'System.Web.Mvc'
? Could not load file or assembly 'Ninject'
```

### Fix:
**Auto-regenerate all binding redirects:**

```powershell
# In Package Manager Console
Add-BindingRedirect
```

**Or manually check Web.config matches packages.config**

---

## ?? **ISSUE 4: Missing NuGet Packages**

### Symptoms:
```
? The type or namespace name 'Ninject' could not be found
? Package restore failed
```

### Fix:
**Restore packages:**

```powershell
# In Package Manager Console
Update-Package -Reinstall
```

**Or right-click solution:**
- Restore NuGet Packages

---

## ?? **ISSUE 5: Build Errors**

### Symptoms:
```
? CS0246: Type or namespace not found
? CS0103: The name 'X' does not exist
```

### Fix:
1. Check `FIX_BUILD_ERRORS.md`
2. Verify all packages restored
3. Clean and rebuild
4. Check references in `.csproj` file

---

## ?? **ISSUE 6: Runtime Errors (500 Internal Server Error)**

### Symptoms:
```
? HTTP Error 500.0 - Internal Server Error
? The page cannot be displayed
```

### Common Causes & Fixes:

**1. Database Connection Issue:**
```xml
<!-- Check Web.config -->
<connectionStrings>
  <add name="DefaultConnection" 
       connectionString="..." />
</connectionStrings>
```

**2. Missing Controller Action:**
- Check requested URL matches a controller action
- Check route configuration in `RouteConfig.cs`

**3. Exception in Code:**
- Check NLog logs in `Logs/` folder
- Enable detailed errors in Web.config:
```xml
<system.web>
  <customErrors mode="Off" />
</system.web>
```

**4. Missing View:**
- Check view file exists in correct folder
- Check view name matches action name

---

## ?? **ISSUE 7: Login/Authentication Not Working**

### Symptoms:
```
? Login button does nothing
? 404 error when submitting login
```

### Fix:

**1. Check controller action exists:**
```csharp
// In HomeController or MarketplaceController
[HttpPost]
public async Task<JsonResult> LoginUser(string email, string password)
{
    // Login logic
}
```

**2. Check AJAX URL is correct:**
```javascript
// In view JavaScript
$.ajax({
    url: '@Url.Action("LoginUser", "Home")',  // Check this!
    type: 'POST',
    data: { email: email, password: password }
});
```

**3. Check Forms Authentication is enabled:**
```xml
<!-- Web.config -->
<authentication mode="Forms">
  <forms loginUrl="~/Home/Index" timeout="480" />
</authentication>
```

---

## ?? **ISSUE 8: Enhanced Auth UI Not Showing**

### Symptoms:
```
? Click Login, nothing happens
? Old form still showing
```

### Fix:

**1. Check partial view is included:**
```cshtml
<!-- In Views/Home/Index.cshtml -->
<div id="authPage" class="hidden">
    @Html.Partial("_EnhancedAuth")
</div>
```

**2. Check JavaScript function exists:**
```javascript
// Should be in _EnhancedAuth.cshtml
function showEnhancedLogin() {
    // Show logic
}
```

**3. Check button onclick:**
```html
<button onclick="showEnhancedLogin(); return false;">Login</button>
```

---

## ?? **ISSUE 9: CSS/Styles Not Applied**

### Symptoms:
```
? Page looks plain, no styling
? Tailwind classes not working
```

### Fix:

**1. Check Tailwind CDN is loaded:**
```html
<!-- In _Layout.cshtml <head> -->
<script src="https://cdn.tailwindcss.com"></script>
```

**2. Check custom styles are after Tailwind:**
```html
<script src="https://cdn.tailwindcss.com"></script>
<script>
    tailwind.config = { /* config */ };
</script>
<style>
    /* Custom styles */
</style>
```

**3. Clear browser cache:**
- Ctrl+F5 (hard refresh)
- Or Ctrl+Shift+Delete ? Clear cache

---

## ?? **ISSUE 10: JavaScript Errors**

### Symptoms:
```
? Console shows "X is not defined"
? AJAX calls failing
```

### Fix:

**1. Check console for errors:**
- Press F12
- Click "Console" tab
- Look for red errors

**2. Check jQuery is loaded:**
```html
<script src="~/Scripts/jquery-3.7.1.min.js"></script>
```

**3. Check script order:**
```html
<!-- Correct order -->
<script src="jquery"></script>
<script src="your-script.js"></script>
```

---

## ?? **ISSUE 11: File Upload Not Working**

### Symptoms:
```
? Images not uploading
? AWS S3 errors
```

### Fix:

**1. Check AWS credentials are set:**
```xml
<!-- Web.config -->
<add key="AWS_S3_BUCKET" value="your-bucket" />
<add key="AWS_S3_ACCESS_KEY_ID" value="YOUR-KEY" />
<add key="AWS_S3_SECRET_KEY" value="YOUR-SECRET" />
```

**?? SECURITY WARNING:** These are exposed! See `SECURITY_ALERT_EXPOSED_CREDENTIALS.txt`

**2. Check FileUploadService is registered:**
```csharp
// In NinjectWebCommon.cs
kernel.Bind<IFileUploadService>().To<FileUploadService>();
```

---

## ?? **ISSUE 12: Database Connection Failed**

### Symptoms:
```
? SqlException: Cannot open database
? Login failed for user
```

### Fix:

**1. Check connection string:**
```xml
<!-- Web.config -->
<connectionStrings>
  <add name="DefaultConnection" 
       connectionString="data source=(localdb)\MSSQLLocalDB;
                        initial catalog=sellnlist;
                        integrated security=True" />
</connectionStrings>
```

**2. Check database exists:**
- Open SQL Server Object Explorer
- Check if database "sellnlist" exists
- Run migrations if using Entity Framework

**3. Check SQL Server is running:**
- Services ? SQL Server (MSSQLLOCALDB) ? Running

---

## ?? **ISSUE 13: Performance Issues**

### Symptoms:
```
?? Page loads slowly
?? High CPU usage
```

### Fix:

**1. Enable output caching:**
```csharp
[OutputCache(Duration = 300)] // 5 minutes
public ActionResult Index()
{
    // Action
}
```

**2. Optimize images:**
- Use WebP format
- Add lazy loading: `<img loading="lazy" />`
- Compress images

**3. Enable GZIP compression:**
```xml
<!-- Web.config -->
<system.webServer>
  <urlCompression doStaticCompression="true" 
                  doDynamicCompression="true" />
</system.webServer>
```

---

## ?? **ISSUE 14: NLog Not Logging**

### Symptoms:
```
? No logs in Logs/ folder
? Exceptions not being logged
```

### Fix:

**1. Check NLog.config exists:**
- Should be in project root
- Should be set to "Copy if newer"

**2. Check file permissions:**
- Logs folder must have write permissions
- Run Visual Studio as Administrator

**3. Check logging is configured:**
```csharp
// Should be in GlobalExceptionFilter.cs
private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
Logger.Error(exception, "Error occurred");
```

---

## ? **CURRENT PROJECT STATUS**

### ? **What's Working:**
- Build: Successful ?
- Newtonsoft.Json: Fixed ?
- Views/Web.config: Updated ?
- All view files: Created ?
- Documentation: Complete ?

### ?? **What Needs Action:**
- Restart Visual Studio (clear IntelliSense cache)
- Implement Phase 1 (Enhanced Auth)
- Create Profile and Messages controllers
- Update security packages (jQuery, Newtonsoft.Json)
- Rotate AWS credentials

---

## ?? **Complete Documentation Index**

### **Setup & Configuration:**
1. `QUICK_START.md` - Project quick start
2. `PROJECT_REVIEW.md` - Project overview
3. `COMPREHENSIVE_PROJECT_REVIEW.md` - Detailed review
4. `Install-Packages.ps1` - Package installation script
5. `INSTALL_COMMANDS.bat` - Batch install commands

### **Refactoring:**
6. `REFACTORING_GUIDE.md` - Refactoring steps
7. `REFACTORING_COMPLETE.md` - Refactoring summary
8. `ACTION_CHECKLIST.md` - Implementation checklist

### **UI Modernization:**
9. `UI_MODERNIZATION_PLAN.md` - Complete UI plan
10. `UI_QUICK_START.md` - 15-minute quick start
11. `ENHANCED_UI_IMPLEMENTATION_GUIDE.md` - Detailed implementation
12. `PHASES_2-5_IMPLEMENTATION_GUIDE.md` - Phases 2-5 guide
13. `UI_MODERNIZATION_SUMMARY.md` - UI status summary
14. `UI_MODERNIZATION_CHECKLIST.md` - Progress checklist
15. `ALL_PHASES_COMPLETE_SUMMARY.md` - Final summary
16. `BUILD_STATUS_AND_NEXT_STEPS.md` - Current status

### **Troubleshooting:**
17. `FIX_BUILD_ERRORS.md` - Build error fixes
18. `FIX_RAZOR_VIEW_ERRORS.md` - IntelliSense fixes
19. `FIX_NEWTONSOFT_JSON_ERROR.md` - Assembly binding fix
20. `FORCE_CLEAN_BUILD.ps1` - Clean build script
21. **THIS FILE** - Complete troubleshooting guide

### **Security:**
22. `SECURITY_ALERT_EXPOSED_CREDENTIALS.txt` - Security warnings
23. `UPDATE_SECURITY_PACKAGES.txt` - Package updates
24. `Web.config.template` - Secure config template

---

## ?? **READY TO START?**

If all issues are resolved, follow this path:

```
1. Restart Visual Studio
   ??> Clear IntelliSense cache
   
2. READ: BUILD_STATUS_AND_NEXT_STEPS.md
   ??> Understand current status
   
3. FOLLOW: UI_QUICK_START.md
   ??> Implement Phase 1 (15 minutes)
   
4. TEST: Enhanced authentication
   ??> Click Login, see modern UI
   
5. CONTINUE: PHASES_2-5_IMPLEMENTATION_GUIDE.md
   ??> Implement remaining phases
   
6. TRACK: UI_MODERNIZATION_CHECKLIST.md
   ??> Mark tasks complete
```

---

## ?? **Pro Tips**

1. **Always rebuild after config changes**
2. **Check browser console (F12) for JavaScript errors**
3. **Use NLog logs for server-side debugging**
4. **Hard refresh browser (Ctrl+F5) after CSS changes**
5. **Focus on build output, not IntelliSense errors**

---

## ?? **Still Stuck?**

If you've tried everything and still have issues:

1. **Check specific error message** in this guide
2. **Search the error code** (e.g., "HRESULT 0x80131040")
3. **Check relevant documentation file** from index
4. **Verify all prerequisites** are met
5. **Try clean build** with `FORCE_CLEAN_BUILD.ps1`

---

## ?? **You've Got This!**

With this complete troubleshooting guide, you can solve any issue that comes up. Remember:

- **Build Success = Code is Correct** ?
- **IntelliSense Errors = Cache Issues** (restart VS)
- **Runtime Errors = Check logs and console**
- **When in doubt, rebuild!**

**Happy coding!** ??

---

*Complete Troubleshooting Guide*  
*ReXell Marketplace*  
*Created: January 2025*
