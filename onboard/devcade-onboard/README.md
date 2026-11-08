# devcade-onboard
administrator software to fix the inablility of `bootc` to handle rapid testing and switching of the other onboard software (`devcade_frontend` and `devcade_backend`)

## Main points
relates to the `devcade_onboard` bash file (will probability replace it), cause it would start and handle the running of the two executable's `devcade_frontend` and `devcade_backend`

another point of it would to be to handle changing the front-end / back-end from stable to some developer instance (branch name or commit of the `devcade_onboard` or to another upstream git repository)
*note: let `bootc` handle the stable release updates via `bootc upgrade`*

another thing that would be incredibly useful is:  
web access for much easier config changes
- requires network connection stuff to communicate with some website
- or it serves its own website
	- yay more authentication work

### babbling
enter main function
read environment file / variables for whether to be on stable or dev
if dev:
1. get which commit (or branch) and git repository to be on
2. clone git repository (or switch to relevant branch)
3. rebuild relevant executables
4. launch updated executables

if stable:
1. launch "normal" executables (the ones under /usr/bin)

enter loop:
1. check executable's status 
2. if either crash
	1. restart both in order
	2. if they fail to start reboot machine
3. if config options are changed and `restart on change` is true
	1. restart executable's
4. j loop