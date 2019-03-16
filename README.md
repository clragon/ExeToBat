## ExeToBat
A project to aim towards embedding any amount of files into a batch file which will extract them on runtime.

There are options for the extraction directory, for running the files after extraction and for deleting after running them.

It uses a console interface which allows adding files or entire folders. Each file can be given an extraction position and for each file the options named above can be configured.

The input files are converted into base64 and split optimally to fit the maximum size of a file writer stream in batch.

After adding all files, the generation option should be used. The executable will generate the batch file in the same folder.
