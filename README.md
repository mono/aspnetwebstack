Clone of https://git01.codeplex.com/aspnetwebstack.git
======================================================

This is a Mono copy of the Microsoft ASP.NET Web Stack (MVC, Razor etc) in order to
make maintenance of the Mono version of those libraries easier.

Mono git submodules maintenance
==============================

Please read http://mono-project.com/Git_Submodules_Maintenance

Maintenance and development
===========================

These instructions are for developers working on code in this repository. End users do not need to be
concerned with the procedures described below.

First clone the repository (do not do any development in the submodule directory in mono source tree).

In order to modify the upstream source code you need a way to interact with the upstream repository in order
to fetch updates, cherry pick or merge them etc. To make it possible you need to add the upstream remote to
your clone of the repository:

    git remote add upstream https://git01.codeplex.com/aspnetwebstack.git

When there are upstream changes you're interested in merging, fetch them but do not apply them to the tree:

    git fetch upstream/master

You can replace 'master' with name of the remote branch you need to update from.

To merge all upstream updates, do:

    git merge upstream/master

followed by

    git push origin/master

After that is done and you're ready to make the changes visible to mono, go to your mono repository clone,
make sure the submodules are initialized and up to date:

    git submodule init
    git submodule update

then go to he external/aspnetwebstack directory and get the changes:

    git pull

Then go back to the top of your copy of mono repository and update the submodule reference:

    git add external/aspnetwebstack
    git push
