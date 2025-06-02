#deployment script
today=$(date +"%Y.%m.%d-%H.%M")
sudo cp -r /datadrive/BFLRewards/EventManagerWorker /datadrive/BackupCodeBFLRewards/EventManagerWorker${today}.bkp
#sudo apt-get update
sudo rm -rf /datadrive/BFLRewards/EventManagerWorker/*
sudo mv /home/azureadmin/azagent/_work/r61/a/_BFLRewards-EventManagerWorker-QA-CI/drop/EventManagerWorker.zip /datadrive/BFLRewards/EventManagerWorker/
cd /datadrive/BFLRewards/EventManagerWorker/
sudo unzip EventManagerWorker.zip
sudo rm -r EventManagerWorker.zip
sudo chmod -R 755 *
sudo systemctl reload nginx.service
sudo systemctl restart BFLRewardsEventManagerWorker.service