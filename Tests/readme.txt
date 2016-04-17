The files in this folder exercise various error recovery conditions.

20973-gon_v03.rar : this file is the first instance of a "zero" image file encountered.
Page 72 of the archive is an image file, not zero length, but whose contents are all zero.
Next and Previous page, as well as Goto Page, should all recover correctly when page 72
is reached.

multi_zero_end.zip : an archive with multiple empty files at the end. Next Page
should correctly refuse to go past the last valid file. 'Goto Page' should correctly
show only the last valid file.

multi_zero_start.zip : an archive with multiple empty files at the start. Prev Page
should correctly refuse to go past the first valid file. 'Goto Page' should correctly
show only the first valid file. Loading the archive should show the first valid file.

60821-Bye_Bye_Love_Letter_SL.zip : this file is the first instance I encountered where
a FOLDER had an image extension. I.e. the folder inside the archive has a .PNG 
extension. The code was changed to not treat folders as images.
