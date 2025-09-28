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
    
    # make frontend executable
    # onboard script expects an executable named frontend
    # preset name must be one defined in 'export_presets.cfg'
    # @see https://docs.godotengine.org/en/latest/tutorials/editor/command_line_tutorial.html
    subprocess.run(f"godot --export-release Linux {out_path}/frontend", shell=True)
    
    # build and move backend
    # onboard script expects an executable named backend
    os.chdir("./backend")
    subprocess.run("cargo build -r", shell=True)
    shutil.move("./target/release/backend", f"{out_path}/")
    
    os.chdir("..")
    # copy onboard shell script (definitely should add this to git lmao)
    shutil.copy2("./onboard", f"{out_path}/onboard")


if __name__ == '__main__':
    main()