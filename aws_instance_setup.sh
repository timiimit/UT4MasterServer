exit

###################################
# DO NOT RUN THIS SCRIPT DIRECTLY #
# IT IS ONLY MEANT AS A GUIDE TO  #
# PREPARE EVERYTHING NECCESSERY   #
###################################

# if you have SSH terminal problems you need to upload your
# terminal information to the host.
# to do this:
#  - locally create 'terminfo' file with 'infocmp > terminfo' command
#  - upload the file to target host
#  - run 'tic terminfo' on the host in the same directory as the file resides

# everything will be stored in /app
# optionally mount a separate volume here
sudo mkdir /app
sudo chown ec2-user:ec2-user /app
cd /app

# install required packages
sudo yum update -y
sudo yum -y git docker httpd mod_ssl

# make docker daemon start at boot and start it now
sudo systemctl enable docker && sudo systemctl restart docker

# now go to /etc/httpd/conf/httpd.conf and locate "Listen 80"
# right after it add the following:
# <VirtualHost *:80>
# 	DocumentRoot "/var/www/html"
# 	ServerName "<PUT_BASE_DOMAIN_NAME_HERE>"
# </VirtualHost>

# make sure httpd (apache) daemon starts at boot
sudo systemctl enable httpd & sudo systemctl restart httpd

# enable packages which are needed to install certbot
sudo wget -r --no-parent -A 'epel-release-*.rpm' https://dl.fedoraproject.org/pub/epel/7/x86_64/Packages/e/
sudo rpm -Uvh dl.fedoraproject.org/pub/epel/7/x86_64/Packages/e/epel-release-*.rpm
sudo yum-config-manager --enable epel*
sudo rm -rf dl.fedoraproject.org/

# install certbot
sudo amazon-linux-extras install epel -y
sudo yum install -y certbot python2-certbot-apache

# get source code of master server (use your own fork if you have a custom version)
git clone https://github.com/timiimit/UT4MasterServer

cd UT4MasterServer
