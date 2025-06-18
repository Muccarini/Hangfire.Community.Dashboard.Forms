﻿# Changelog

All notable changes to this project will be documented in this file.

## [2.3.2] - 2024-1-17

## Changed
- Migrated javascript libraries to use npm instead of ones included in the library project.
- Altered the way that client resources are loaded using a new dispatcher class
- Added `connect-src` option to CSP for ASP.Net Core's hot reloading used in the example site.

## Added
- Added configuration options for client side DateTime controls and "Task Type: Scheduled" execution option
  - See ASP.Net Core Web Application's \Startup.cs and \HangfireManagement\Expedited.cs files for examples.

## Fixed
- Scheduling a job when the `DisplayName` attribute is used with non-route friendly characters.

## [2.3.1] - 2024-1-10

### Fixed
- Bug in DisplayDataAttribute for the isRequired flag ([SindreMA](https://github.com/SindreMA))
- Finished Multiline input support ([SindreMA](https://github.com/SindreMA))
- Schedule Job double conversion ([DanPatten](https://github.com/DanPatten))
- build badge in readme


## Changed
- Update `jQuery.Validation` from `1.19.3` to `1.19.4` in ASP.Net example site (dependabot)
- Update `Microsoft.Owin` from `4.2.0` to `4.2.2` in ASP.Net example site (dependabot)

## [2.3.0] - 2024-01-09

I have recently had some time to focus on this project again and have tried to clear up some issues
that have been opened.  The main change for this release is that it should now be CSP compliant!

### Added
- This CHANGELOG file
- Added CPS headers to both sample web projects

### Fixed
- `jQuery.InputMask` has been replaced with [`Inputmask`](https://github.com/RobinHerbots/Inputmask) to fix CSP issues.
- `Bootstrap DropDown` has been replaced with [`Tempus Dominus`](https://github.com/Eonasdan/tempus-dominus) to fix CSP issues.

### Changed
- Reworked how pages were generated to be more in line with the way that Hangfire.Core is doing it.
- Changed how DateTime objects are passed to the API endpoint. They are now passed as UTC from the client and should passed to the Job's method as UTC.

### Removed
Since I was not able to make the original Cron Builder UI library work the way I believe that it should, I have removed it.
The ability to schedule recurring jobs based on Cron is __still available__, but the Cron Builder UI is no longer there.
I will look at adding it back sometime in the future, or feel free to submit a pull request with a __CSP compliant__ builder.

You can use an online Cron Expression Builder (examples below) to build the string that you can enter in the Management UI.

[`CronMaker`](http://www.cronmaker.com/)

[`Cron Expression Generator & Explainer - Quartz`](https://www.freeformatter.com/cron-expression-generator-quartz.html)

[`crontab guru`](https://crontab.guru/)
