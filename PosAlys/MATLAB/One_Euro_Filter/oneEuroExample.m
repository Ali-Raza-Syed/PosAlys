clear
%Example signal noisy sine wave
t = -pi:0.01:pi;
x = linspace(-pi, pi, length(t));
%noisySignal = sin(x)+0.5*rand(1, length(x));
ns = sin(x)+0.5*rand(1, length(x));
noisySignal = zeros(2, 21, length(x));
for row_index = 1 : 2
   for col_index = 1 : 21
       noisySignal(row_index, col_index, :) = ns;
   end
end

%Declare oneEuro object
a = oneEuro;
%Alter filter parameters to tune
a.mincutoff = 1.0;
a.beta = 0.0;


filteredSignal = zeros(size(noisySignal));
c = size( noisySignal );
for i = 1:c(3)
    %filteredSignal(i) = a.filter(noisySignal(i),i);
    filteredSignal(:, :, i) = a.filter(noisySignal(:, :, i),i);
end

a = reshape( noisySignal(2, 1, :), 1, 629 );
b = reshape( filteredSignal(2, 1, :), 1, 629 );
%plot(t, noisySignal);
plot(t, a);
hold on;
%plot(t, filteredSignal);
plot(t, b);