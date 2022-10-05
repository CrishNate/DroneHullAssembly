@echo off
cd venv/Scripts/
mlagents-learn --torch-device "cuda" --force ../../config/ppo/Drone.yaml --num-envs 4 --env ../../env/DroneHullAssembly.exe --width 84 --height 84 --no-graphics
pause