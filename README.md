# NinjaForge

[NinjaForge Github Repo](https://github.com/MarkL1213/NinjaForge)

This project is licensed under the terms of the MIT license.

## Command Line
Command line options:
- Options can be prefixed by "/", "-", or "--".
- All options must be specified separately. Short name bundling is not supported.

| Argument Name | Short Name | Required Value  | Description |
|:-------------:|:---:|:----------------:|:------------|
|launch       | l | *\<workspace name\>* |Launch the specified workspace.|
|safe         | s |                 |Use safe mode.|
|help         | h |                 |Display usage help.|

## Features
Safe Mode:
- Launch NinjaTrader using the safe mode option.

Cleanup:

The following folders are cleaned of **all files** when running the associated clean option.

- Reflection Cache:
   - <%MyDocuments%>\NinjaTrader 8\cache\
- Log Files:
   - <%MyDocuments%>\NinjaTrader 8\log\
- Trace Files:
   - <%MyDocuments%>\NinjaTrader 8\trace\
- Price Data:
   - <%MyDocuments%>\NinjaTrader 8\db\tick\
   - <%MyDocuments%>\NinjaTrader 8\db\minute\
   - <%MyDocuments%>\NinjaTrader 8\db\day\
   - <%MyDocuments%>\NinjaTrader 8\db\replay\
   - <%MyDocuments%>\NinjaTrader 8\db\cache\
- Analyzer Logs:
   - <%MyDocuments%>\NinjaTrader 8\strategyanalyzerlogs\


