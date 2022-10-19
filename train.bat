@echo off
cd venv/Scripts/
mlagents-learn --torch-device "cuda" --force ../../config/ppo/Drone.yaml --num-envs 4 --env ../../env/DroneHullAssembly.exe --no-graphics --width 84 --height 84 --env-args %*
pause