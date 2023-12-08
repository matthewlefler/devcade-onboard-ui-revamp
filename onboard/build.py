#!/usr/bin/python3


def main():
    import os
    import shutil
    import subprocess
    
    home_path = "/home/devcade"
    out_path = "/home/devcade/publish"
    
    
    if os.path.exists(f"{home_path}/publish.bak") and os.path.exists(out_path):
        shutil.rmtree(f"{home_path}/publish.bak")
    
    if os.path.exists(out_path):
        shutil.move(out_path, f"{home_path}/publish.bak")
    
    
    # build and move frontend
    os.chdir("./frontend")
    subprocess.run("dotnet publish -c Release -r linux-x64 --sc", shell=True)
    shutil.move("./bin/Release/net6.0/linux-x64/publish", home_path)
    shutil.move(f"{out_path}/onboard", f"{out_path}/frontend")
    
    # build and move backend
    os.chdir("../backend")
    subprocess.run("cargo build -r", shell=True)
    shutil.move("./target/release/backend", f"{out_path}/")
    
    os.chdir("..")
    # copy onboard shell script (definitely should add this to git lmao)
    shutil.copy2("./onboard", f"{out_path}/onboard")


if __name__ == '__main__':
    main()