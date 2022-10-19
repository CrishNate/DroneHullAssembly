@echo off
cd venv/Scripts/
mlagents-learn --torch-device "cuda" --force ../../config/ppo/Drone.yaml
pause