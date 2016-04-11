# GrepSearch

Unix(ish) globally search a regular expression and print, with windows forms and command line console user interfaces.

Credit to George Anescu and Stelios Alexandrakis who contributed the following articles.
http://www.codeproject.com/Articles/1485/A-C-Grep-Application
http://www.codeproject.com/Articles/31105/A-ComboBox-with-a-CheckedListBox-as-a-Dropdown

Also R. Lopes' CommandLine.Utility.Arguments class: "application arguments interpreter" also used by G. Anescu in his ConsGrep project.

From G. Anescu contribution the only part that remains of his original project is the form layout and some of the variable\class names. Likewise I also kept the console applications command line argument parameters unchanged.
His project failed to execute because cross threaded access to the win forms controls.

I did rely heavily upon S. Alexandrakis CheckedComboBox project. With the biggest changes made to the way CheckedListBox.ItemCheck event was handled, and some format refactoring for clarity.

Other than these noted elements the entirety of this work is my own.

This S. Alexandrakis CheckedComboBox project was extremely useful to produce the effect of a combo box with file extensions that could be chosen via a check box.

Furthermore I used this project to implement a way to:
- uncheck the default extensions "*.*" when any other extension is chosen.
- when all other extensions are unchecked the default extension gets checked again.
- when the default extension is checked all other checked extensions are un-checked.
- if no other extensions are checked the default extension can not be un-checked.

The heavy lifting of this project is accomplished in the GrepSearch.GrepWorker class where a BackgroundWorker scans and fires events that indicate progress.

There are 7 grep events.
GrepFileChanged: fires each time a file scan starts, indicating the file path that is being scanned.
GrepFileHasMatch: fires the first time a matching line is found for a file, indicating the file path containing the matching line.
GrepResult: fires each time a matching line is found, indicating the file path containing the matching line, the line number that contains the match, and the text of that matching line.
GrepFinishedFile: fires each time a file scan has completed, indicating the file path of the completed file and the number of matching lines that file contained.
GrepException: fires when an error occurs, indicating an exception object and whether this exception was cause to terminate the background worker.
GrepCanceled: fires when the background worker has been cancelled prior to completing on its own.
GrepComplete: fires when the background worker has completed on its own.

To begin a grep search: instantiate a GrepWorker class object and pass the following parameters to its Start method.
  1. directory: the directory to begin searching from.
  2. filePattern: a windows style file name pattern with optional wildcards. (all files "*.*" for WinGrep)
  3. extensions: a list of extensions in the form "*.ext", null or "*.*" means all extensions.
  4. regex: the regular expression pattern to find matches for each line in the file being scanned.
  5. isRecursive: true to search subdirectories false to search top directory only.
  6. ignoreCase: true for case insensitive searches, false to exactly match case.
  7. fileNameOnly: true to return file name without line match data (stops scanning file at first matching line). false to return every matching line in the file.

Besides these parameters each UI also has switches for showing line numbers and counting lines. And as G. Anescu had done, these switches are cancelled out by the fileNamesOnly parameter.

TODO:
I believe at some point I will add another project WPFGrep to employ a windows presentations foundation MVVM pattern implementation to this solution.
