#!/usr/bin/env bash
sudo iptables -t nat -A OUTPUT -p tcp --dport 9958 -j DNAT --to-destination 192.168.0.10:9958
sudo iptables -t nat -A OUTPUT -p tcp --dport 5816 -j DNAT --to-destination 192.168.0.10:5816
