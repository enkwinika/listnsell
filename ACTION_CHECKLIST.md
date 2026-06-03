# ? ACTION CHECKLIST - REFACTORING COMPLETION

## ?? CRITICAL - DO TODAY

### 1. Security: Exposed AWS Credentials
Status: [ ] Not Started

**Why Critical:** Your AWS credentials are exposed and could be compromised
**Time Required:** 15-30 minutes

Steps:
- [ ] Login to AWS Console (https://console.aws.amazon.com)
- [ ] Navigate to IAM ? Users ? [Your User] ? Security Credentials
- [ ] Find access key: [AWS_ACCESS_KEY_ID]
- [ ] Click "Make inactive" then "Delete" on this key
- [ ] Click "Create access key" to generate new credentials
- [ ] Copy new Access Key ID and Secret Access Key
- [ ] Open Web.config in Visual Studio
- [ ] Replace AWS_S3_ACCESS_KEY_ID with new value
- [ ] Replace AWS_S3_SECRET_KEY with new value
- [ ] Save Web.config
- [ ] Test file upload functionality still works

**Reference:** SECURITY_ALERT_EXPOSED_CREDENTIALS.txt

---

### 2. Security: Rotate Database Password
Status: [ ] Not Started

**Why Critical:** Database password is exposed in Web.config
**Time Required:** 10-15 minutes

Steps:
- [ ] Connect to SQL Server: taskmaster.aserv.co.za,1434
- [ ] Change password for user: telolqzk_user
- [ ] Update Web.config ? DefaultConnectionLive ? password=YOUR_NEW_PASSWORD
- [ ] Test database connection works

---

### 3. Security: Update Vulnerable Packages
Status: [ ] Not Started

**Why Critical:** jQuery has moderate severity, Newtonsoft.Json has HIGH severity vuln
**Time Required:** 5-10 minutes

Steps:
- [ ] Open Visual Studio
- [ ] Go to Tools ? NuGet Package Manager ? Package Manager Console
- [ ] Run: `Update-Package jQuery -Version 3.7.1`
- [ ] Wait for completion
- [ ] Run: `Update-Package Newtonsoft.Json -Version 13.0.3`
- [ ] Wait for completion
- [ ] Clean Solution (Build ? Clean Solution)
- [ ] Rebuild Solution (Build ? Rebuild Solution)
- [ ] Verify build succeeds with no errors
- [ ] Test application still works

**Reference:** UPDATE_SECURITY_PACKAGES.txt

---

### 4. Security: Protect Web.config from Source Control
Status: [ ] Not Started

**Why Critical:** Prevent future credential exposure
**Time Required:** 5 minutes

Steps:
- [ ] Open .gitignore file in your repository root
- [ ] Add these lines:
      ```
      # Configuration files with secrets
      Web.config
      Web.config.secrets
      *.config.secrets
      
      # Keep the template
      !Web.config.template
      ```
- [ ] Save .gitignore
- [ ] If Web.config was previously committed:
      - [ ] Remove from git history (see SECURITY_ALERT_EXPOSED_CREDENTIALS.txt)
      - [ ] Force push to remote to rewrite history
- [ ] Share Web.config.template with your team instead

---

## ?? HIGH PRIORITY - DO THIS WEEK

### 5. Configure NLog File Output
Status: [ ] Not Started

**Why Important:** Logs won't be written without this setting
**Time Required:** 1 minute

Steps:
- [ ] In Visual Studio Solution Explorer, find NLog.config
- [ ] Right-click NLog.config ? Properties
- [ ] Find "Copy to Output Directory"
- [ ] Change to "Copy if newer"
- [ ] Save
- [ ] Rebuild solution
- [ ] Run application
- [ ] Check for Logs folder in bin/Debug or bin/Release
- [ ] Verify log files are created (app.log, error.log, console.log)

---

### 6. Implement Admin Role Check
Status: [ ] Not Started

**Why Important:** Admin endpoints currently only check authentication, not admin role
**Time Required:** 30-60 minutes

Steps:
- [ ] Open Filters/ValidationAttributes.cs
- [ ] Locate AuthorizeAdminAttribute class
- [ ] Add method to check if user is admin:
      ```csharp
      private bool IsUserAdmin(int userId)
      {
          // Option 1: Check database for admin role
          // Option 2: Check a specific admin table
          // Option 3: Check if userId matches known admin IDs
          // Implement based on your database schema
      }
      ```
- [ ] Update OnActionExecuting to call IsUserAdmin
- [ ] Return 403 Forbidden if not admin
- [ ] Test with non-admin user
- [ ] Test with admin user
- [ ] Verify access control works correctly

**Current Location:** Line ~75 in Filters/ValidationAttributes.cs

---

### 7. Test Application End-to-End
Status: [ ] Not Started

**Why Important:** Verify refactoring didn't break functionality
**Time Required:** 1-2 hours

Test Checklist:
- [ ] **Registration Flow**
  - [ ] Register new user
  - [ ] Receive email verification code
  - [ ] Verify email with code
  - [ ] Check user is activated

- [ ] **Authentication Flow**
  - [ ] Login with correct credentials
  - [ ] Logout
  - [ ] Login again
  - [ ] Try logging in with wrong password
  - [ ] Test "Forgot Password" flow
  - [ ] Test password reset

- [ ] **Listing Management**
  - [ ] Create new listing
  - [ ] Upload images (test with multiple files)
  - [ ] View listing details
  - [ ] Edit existing listing
  - [ ] Delete listing
  - [ ] Mark listing as sold

- [ ] **Search and Browse**
  - [ ] Browse all listings
  - [ ] Filter by category
  - [ ] Search for specific items
  - [ ] View listing details page

- [ ] **Admin Functions** (if you have admin access)
  - [ ] View admin dashboard
  - [ ] See pending listings
  - [ ] Approve a listing
  - [ ] Reject a listing
  - [ ] View reported listings
  - [ ] Dismiss a report
  - [ ] View all users
  - [ ] Toggle user status (activate/deactivate)

- [ ] **Messages** (if implemented)
  - [ ] Send message about listing
  - [ ] View received messages
  - [ ] Reply to message

Issues Found During Testing:
```
[List any issues you discover here]




```

---

### 8. Review Logs for Errors
Status: [ ] Not Started

**Why Important:** Catch any runtime issues early
**Time Required:** 15 minutes

Steps:
- [ ] Run application
- [ ] Perform various actions (login, create listing, etc.)
- [ ] Navigate to project directory
- [ ] Open Logs/error.log
- [ ] Check for any ERROR level logs
- [ ] Open Logs/app.log
- [ ] Review INFO and WARN level logs
- [ ] Investigate any unexpected errors
- [ ] Fix issues found

Errors Found in Logs:
```
[List any errors you find here]




```

---

## ?? MEDIUM PRIORITY - DO THIS MONTH

### 9. Set Up Environment Variables (Production)
Status: [ ] Not Started

**Time Required:** 30-45 minutes

**For Azure Web Apps:**
- [ ] Login to Azure Portal
- [ ] Navigate to your App Service
- [ ] Go to Configuration ? Application settings
- [ ] Add these settings:
  - [ ] AWS_S3_BUCKET
  - [ ] AWS_S3_ACCESS_KEY_ID
  - [ ] AWS_S3_SECRET_KEY
  - [ ] MerchantID
  - [ ] ApplicationID
  - [ ] jti
  - [ ] securityKey
  - [ ] Database connection string
- [ ] Save changes
- [ ] Restart App Service
- [ ] Test production application

**For Other Hosting:**
- [ ] Set environment variables on server
- [ ] Update application startup to read from environment
- [ ] Test with environment variables

---

### 10. Production Web.config Hardening
Status: [ ] Not Started

**Time Required:** 15 minutes

Steps:
- [ ] Open Web.config
- [ ] Change `<compilation debug="true"` to `debug="false"`
- [ ] Change `<forms requireSSL="false"` to `requireSSL="true"` (if using HTTPS)
- [ ] Add custom errors:
      ```xml
      <customErrors mode="On" defaultRedirect="~/Error">
        <error statusCode="404" redirect="~/Error/NotFound" />
        <error statusCode="500" redirect="~/Error/ServerError" />
      </customErrors>
      ```
- [ ] Save Web.config
- [ ] Deploy to production
- [ ] Test error pages work

---

### 11. Performance Monitoring Setup
Status: [ ] Not Started

**Time Required:** 1-2 hours

Options:
- [ ] Option A: Azure Application Insights
  - [ ] Install Microsoft.ApplicationInsights.Web NuGet package
  - [ ] Configure ApplicationInsights.config
  - [ ] Deploy and verify telemetry

- [ ] Option B: Custom logging dashboard
  - [ ] Set up log aggregation (e.g., Seq, ELK Stack)
  - [ ] Create dashboard for key metrics
  - [ ] Set up alerts for errors

- [ ] Option C: Third-party APM
  - [ ] Evaluate New Relic, Datadog, or Raygun
  - [ ] Install agent
  - [ ] Configure monitoring

---

### 12. Plan Database Async Conversion
Status: [ ] Not Started

**Time Required:** Planning 1-2 hours, Implementation 8-16 hours

**Current State:**
- CommonCoreService uses Task.Run wrappers around sync methods
- Works but not true async (blocks thread pool threads)

**Future Improvement:**
- [ ] Audit Core/commonCore.cs methods
- [ ] Identify all database calls (Dapper)
- [ ] Convert to async Dapper methods:
  - [ ] Query ? QueryAsync
  - [ ] Execute ? ExecuteAsync
  - [ ] QueryFirst ? QueryFirstAsync
  - [ ] QuerySingle ? QuerySingleAsync
- [ ] Update method signatures to async Task<T>
- [ ] Remove Task.Run wrappers from CommonCoreService
- [ ] Test thoroughly
- [ ] Deploy incrementally

**Priority:** Low (current implementation works, this is optimization)

---

## ?? PROGRESS TRACKER

### Overall Completion
- [ ] Critical Items (4/4 completed)
- [ ] High Priority Items (4/4 completed)
- [ ] Medium Priority Items (4/4 completed)

**Total Progress:** 0/12 items (0%)

Update this checklist as you complete each item!

---

## ?? GETTING HELP

### If You Get Stuck:

**Build Errors:**
- See: FIX_BUILD_ERRORS.md
- Check: All NuGet packages installed
- Try: Clean and Rebuild Solution

**Runtime Errors:**
- Check: Logs/error.log
- Verify: NLog.config copied to output
- Review: Stack traces in log files

**Database Issues:**
- Verify: Connection string is correct
- Test: SQL Server is accessible
- Check: User has proper permissions

**AWS Issues:**
- Verify: New credentials are correct
- Check: S3 bucket exists and is accessible
- Test: IAM user has S3 write permissions

**Authentication Issues:**
- Verify: Forms Authentication configured
- Check: Cookie settings in Web.config
- Test: Database has user records

---

## ?? WHEN COMPLETE

After finishing all checklist items:

? Your application will be:
- **Secure**: No exposed credentials, vulnerabilities patched
- **Maintainable**: Clean architecture with DI and logging
- **Performant**: Async operations, optimized queries
- **Observable**: Comprehensive logging and error tracking
- **Production-Ready**: Hardened configuration, monitoring in place

? You will have:
- Modern ASP.NET MVC application
- Enterprise-grade patterns
- Security best practices implemented
- Foundation for future enhancements

---

**Start with the CRITICAL items today, then work through the rest at your pace.**

Good luck! ??

---

*Checklist created: January 2025*
*ReXell Marketplace Refactoring Project*
