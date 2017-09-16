clear all

filename = 'E:\GitHub\TWS\Data\EUR\2011\2011_H4.csv';
data11 = csvread(filename,0,1);
filename = 'E:\GitHub\TWS\Data\EUR\2012\2012_H4.csv';
data12 = csvread(filename,0,1);
filename = 'E:\GitHub\TWS\Data\EUR\2013\2013_H4.csv';
data13 = csvread(filename,0,1);
filename = 'E:\GitHub\TWS\Data\EUR\2014\2014_H4.csv';
data14 = csvread(filename,0,1);
filename = 'E:\GitHub\TWS\Data\EUR\2015\2015_H4.csv';
data15 = csvread(filename,0,1);
filename = 'E:\GitHub\TWS\Data\EUR\2016\2016_H4.csv';
data16 = csvread(filename,0,1);
filename = 'E:\GitHub\TWS\Data\EUR\2017\2017_H4.csv';
data17 = csvread(filename,0,1);

%join data
data = [data11; data12; data13; data14; data15; data16; data17];

%daily high - daily low
range = (data(:,3)+data(:,4))/2-(data(:,5)+data(:,6))/2;
%get return using close price
ret = price2ret(data(:,1)) * 100;
erange = range - mean(range);
eret = ret - mean(ret);
length = size(range,1);

figure('units','normalized','outerposition',[0 0 1 1])

h1 = subplot(2,2,1);
set(h1, 'Position', [0.03 .55 .45 .42]);
plot(data(:,7))
xlim([0 length])
title('EURUSD H4 close price')

h2 = subplot(2,2,2);
set(h2, 'Position', [0.53 .55 .45 .42]);
autocorr(range,100)
title('range autocorrelation (D1)')

rangeMean = mean(range);
rangeStd = std2(range);
rangeMed = median(range);
h3 = subplot(2,2,3);
set(h3, 'Position', [0.03 0.06 .45 .42]);
plot(range)
xlim([0 length])
title('range(mid high - mid low)')
text(10,max(range),sprintf('Mean: %f\nStd: %f\nMedian :%f', rangeMean, rangeStd, rangeMed))
