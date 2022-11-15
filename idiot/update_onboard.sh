echo "Pulling from remote"
cd ~/git/Devcade-Onboard
git pull 1>/dev/null
cd onboard

# if publish dir already exists back it up
if [ -d ~/publish ]
then
  mv ~/publish ~/publish.bak
  rm -rf ~/publish
fi

# clean bin directory (probably not necessary)
if [ -f ~/git/Devcade-onboard/onboard/bin ]
then
  # clean build dir
  rm -rf ~/git/Devcade-onboard/onboard/bin
fi

echo "Building..."
# if build completes successfully
dotnet publish -c release -r linux-x64 --self-contained 1>/dev/null
rc=$?
if [[ $rc -eq 0 ]]
then
  echo "Build completed successfully"
  # move published build to correct dir
  mv ~/git/Devcade-Onboard/onboard/bin/Release/net6.0/linux-x64/publish ~/
  # cleanup backup
  rm -rf ~/publish.bak
  chmod u+x ~/publish/onboard
else
  # otherwise, shit's fucked
  echo "Build finished with errors"
  mv ~/profile.bak ~/profile
fi

cd ~
