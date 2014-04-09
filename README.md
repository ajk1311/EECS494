READ ME MUTHA FUCKA!
UNLESS YOU'RE A BASIC BITCH


Check this out:
Path Failed : Computation Time 0.00 ms Searched Nodes 0
Error: Canceled path because a new one was requested.
This happens when a new path is requested from the seeker when one was already being calculated.
For example if a unit got a new order, you might request a new path directly instead of waiting for the now invalid path to be calculated. Which is probably what you want.
If you are getting this a lot, you might want to consider how you are scheduling path requests.
