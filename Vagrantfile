# -*- mode: ruby -*-
# vi: set ft=ruby :

Vagrant.configure(2) do |config|
  #config.vm.box = "debian/jessie64"
  config.vm.box = "debian/wheezy64"
  config.vm.network "forwarded_port", guest: 9000, host: 9000
  config.vm.provision :shell, :path => 'bootstrap.sh'
  config.vm.synced_folder ".", "/home/vagrant/Predictions", type: "smb"
end