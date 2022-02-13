# Known Issues

## Failed Responses
Especially with large batch request (~30 parameter sets and more) you might get 
a warning that some requests failed.
This is typically related to the network connection and can have multiple
reasons, most likely are:
- socket hang up
- connection timed out

This depends on a lot of parameters set on the server and client. 
What helps is that these type of network errors are usually transient, 
thus a simple retry (right away) will likely fix this issue.

Should you experience extreme problems feel free to issue a ticket on 
[github]( https://xtoeffel.github.io/busgs/). 
You will need to create an account if you don't have one already.
Since this might be related to your machine (client settings) it might not
be possible to reproduce the error for debugging.
