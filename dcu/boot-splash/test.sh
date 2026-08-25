sudo bash -c "
    current_splash=$(plymouth-set-default-theme);

    plymouthd --kernel-command-line=splash &&
    plymouth-set-default-theme devcade >/dev/null 2>&1 && 
    plymouth --show-splash; 
    for ((I=0; I<40; I++)); do 
        plymouth --update=test$I ; 
        sleep 0.1; 
    done; 
    plymouth quit;
    
    plymouth-set-default-theme $current_splash >/dev/null 2>&1;
    echo \"done, splash reset to $(plymouth-set-default-theme)\"
"