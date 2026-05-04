while ! modinfo -F version nvidia >/dev/null 2>&1; do
    echo "Waiting for NVIDIA driver..."
    sleep 5
done

echo -e "driver installed:\n {modinfo -F version nvidia}"